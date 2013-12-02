using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Gramdel.Core
{
    /// <summary>
    /// Structure used to indicate the order of an object inside a collection.
    /// </summary>
    [DebuggerDisplay("{GetType().Name}: {ToString()}")]
    public struct Ordinal : IComparable<Ordinal>, IEquatable<Ordinal>
    {
        private readonly uint uint32;

        public Ordinal(int value)
        {
            this.uint32 = value < 0 ? 0u : (uint)value;
        }

        public static implicit operator int(Ordinal value)
        {
            return (int)value.uint32;
        }

        public static implicit operator Ordinal(int value)
        {
            return new Ordinal(value);
        }

        public static implicit operator int?(Ordinal value)
        {
            return value.uint32 == 0 ? null : (int?)(value.uint32);
        }

        public static implicit operator Ordinal(int? value)
        {
            return new Ordinal(value ?? 0);
        }

        public override string ToString()
        {
            return this.uint32 == 0 ? "undefined" : this.uint32.ToString(CultureInfo.InvariantCulture);
        }

        public int CompareTo(Ordinal other)
        {
            return this.uint32.CompareTo(other.uint32);
        }

        public bool Equals(Ordinal other)
        {
            return this.uint32 == other.uint32;
        }

        public override int GetHashCode()
        {
            return this.uint32.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Ordinal && this.Equals((Ordinal)obj);
        }

        public static readonly Ordinal Undefined = new Ordinal();

        public bool IsDefined
        {
            get { return this.uint32 != 0; }
        }

        public bool IsUndefined
        {
            get { return this.uint32 == 0; }
        }
    }
}
