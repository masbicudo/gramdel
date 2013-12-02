using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace Gramdel.Core
{
    public static class CloningUtility
    {
        private static readonly ConcurrentDictionary<Type, object> cloners = new ConcurrentDictionary<Type, object>();

        public static T[] CloneArray<T>(T[] array) where T : class
        {
            if (array == null)
                return null;

            var result = new T[array.Length];
            for (var it = 0; it < array.Length; it++)
                result[it] = CloneObject(array[it]);
            return result;
        }

        public static T CloneObject<T>(T node) where T : class
        {
            if (node == null)
                return null;

            var cloneable = node as ICloneable<T>;
            if (cloneable != null)
                return cloneable.Clone();

            var clonerObj = (Func<T, T>)cloners.GetOrAdd(node.GetType(), EmitCloner);

            return clonerObj(node);
        }

        private static object EmitCloner(Type type)
        {
            var method = new DynamicMethod(string.Format("Clone{0}", type.Name), type, new[] { typeof(object) }, type);
            var il = method.GetILGenerator();

            // create new object of type
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                throw new Exception("Cannot generate cloner. Type must have a default public constructor.");
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc_0);

            // cast argument to proper type
            if (!type.IsValueType)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Stloc_1);
            }
            var opCodeLdValue = type.IsValueType ? OpCodes.Ldarg_0 : OpCodes.Ldloc_1;
            var opCodeLdValue_S = type.IsValueType ? OpCodes.Ldarga_S : OpCodes.Ldloca_S;
            var opCodeLdValue_n = type.IsValueType ? 0 : 1;

            foreach (var eachPropertyInfo in type.GetProperties())
            {
                if (eachPropertyInfo.GetCustomAttributes<ClonePropertyAttribute>(true) != null)
                {
                    var getMethodInfo = eachPropertyInfo.GetGetMethod();
                    var setMethodInfo = eachPropertyInfo.GetSetMethod();

                    if (getMethodInfo != null && setMethodInfo != null)
                    {
                        // read property from source
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(getMethodInfo.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, getMethodInfo);

                        // clone the property
                        if (eachPropertyInfo.PropertyType.IsArray)
                        {
                            var elementType = eachPropertyInfo.PropertyType.GetElementType();
                            if (elementType.IsClass)
                            {
                                il.Emit(OpCodes.Call, typeof(CloningUtility).GetMethod("CloneArray").MakeGenericMethod(elementType));
                            }
                            else if (elementType.IsValueType)
                            {
                                // Creating a new array of the same size, or null if the source is null.
                                // todo: create a new array and clone each item

                                var typeCloneable = typeof(ICloneable<>).MakeGenericType(type);
                                if (elementType.GetInterfaces().Any(typei => typei == typeCloneable))
                                {
                                    // If the value type implements ICloneable<>, use the Clone method of that interface,
                                    // in a constrained method call, because we know the type of the object,
                                    // because nothing can inherit from a value type.
                                    il.Emit(opCodeLdValue_S, opCodeLdValue_n);
                                    il.Emit(OpCodes.Constrained, elementType);
                                    il.Emit(OpCodes.Calli, typeCloneable.GetMethod("Clone"));
                                }
                                else
                                {
                                    // If the value type is not cloneable, just load the value.
                                    il.Emit(opCodeLdValue);
                                }
                            }
                        }
                        else if (eachPropertyInfo.PropertyType.IsClass)
                        {
                            il.Emit(OpCodes.Call, typeof(CloningUtility).GetMethod("CloneObject").MakeGenericMethod(eachPropertyInfo.PropertyType));
                        }
                    }
                }
            }

            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ClonePropertyAttribute : Attribute
    {
    }
}
