using System;
using System.Collections.Generic;

namespace Gramdel.Core
{
    public class TypeDictionary
    {
        private Dictionary<Type, object> dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDictionary"/> class.
        /// </summary>
        public TypeDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDictionary"/> class.
        /// </summary>
        /// <param name="other"> The other dictionary to copy values from. </param>
        public TypeDictionary(TypeDictionary other)
        {
            if (other != null && other.dictionary != null)
                this.dictionary = new Dictionary<Type, object>(other.dictionary);
        }

        /// <summary>
        /// The try get modifier.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <typeparam name="TMod"> </typeparam> <returns>
        /// The <see cref="bool"/>. </returns>
        public bool TryGetModifier<TMod>(out TMod value)
        {
            if (this.dictionary == null)
            {
                value = default(TMod);
                return false;
            }

            object obj;
            bool found = this.dictionary.TryGetValue(typeof(TMod), out obj);
            value = (TMod)obj;
            return found;
        }

        public void SetTypedValue<TMod>(TMod value)
        {
            this.dictionary = this.dictionary ?? new Dictionary<Type, object>();
            this.dictionary[typeof(TMod)] = value;
        }
    }
}