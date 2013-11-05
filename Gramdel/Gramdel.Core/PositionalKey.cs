using System;

namespace Gramdel.Core
{
    internal struct PositionalKey<TKey>
    {
        public PositionalKey(int position, TKey key)
        {
            this.position = position;
            this.key = key;
        }

        public int position;
        public TKey key;

        public override string ToString()
        {
            if (this.key == null) return "Empty";
            return this.position + ": " + (this.key is Delegate ? (this.key as Delegate).Method.Name : this.key as object);
        }
    }
}