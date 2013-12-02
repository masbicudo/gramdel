using System;
using System.Diagnostics;
using System.Linq;

namespace Gramdel.Core
{
    [DebuggerDisplay("{GetType().Name}: {ToString()}")]
    public struct AlternativeKey : IComparable<AlternativeKey>, IEquatable<AlternativeKey>
    {
        private readonly int[] keyItems;

        public AlternativeKey(int items)
        {
            this.keyItems = items == 0 ? null : new int[items];
        }

        public static AlternativeKey operator +(AlternativeKey a, int b)
        {
            var result = new AlternativeKey(a.keyItems.Length + 1);
            Array.Copy(a.keyItems, result.keyItems, a.keyItems.Length);
            result.keyItems[a.keyItems.Length] = b;
            return result;
        }

        public static AlternativeKey operator +(int a, AlternativeKey b)
        {
            var result = new AlternativeKey(b.keyItems.Length + 1);
            Array.Copy(b.keyItems, 0, result.keyItems, 1, b.keyItems.Length);
            result.keyItems[0] = a;
            return result;
        }

        public static AlternativeKey operator +(AlternativeKey a, AlternativeKey b)
        {
            var result = new AlternativeKey(a.keyItems.Length + b.keyItems.Length);
            Array.Copy(a.keyItems, result.keyItems, a.keyItems.Length);
            Array.Copy(b.keyItems, 0, result.keyItems, a.keyItems.Length, b.keyItems.Length);
            return result;
        }

        public override string ToString()
        {
            if (this.keyItems == null || this.keyItems.Length == 0)
                return "[]";

            return string.Format("[{0}]", string.Join("-", this.keyItems.Cast<object>()));
        }

        public bool Equals(AlternativeKey other)
        {
            if (this.keyItems == other.keyItems)
                return true;

            if (this.keyItems == null)
                return false;

            if (other.keyItems == null)
                return false;

            if (this.keyItems.Length != other.keyItems.Length)
                return false;

            for (int it = 0; it < this.keyItems.Length; it++)
                if (this.keyItems[it] != other.keyItems[it])
                    return false;

            return true;
        }

        public int CompareTo(AlternativeKey other)
        {
            if (this.keyItems == other.keyItems)
                return 0;

            if (this.keyItems == null)
                return -1;

            if (other.keyItems == null)
                return 1;

            for (int it = 0; it < this.keyItems.Length && it < other.keyItems.Length; it++)
            {
                var cmp = this.keyItems[it].CompareTo(other.keyItems[it]);
                if (cmp != 0)
                    return cmp;
            }

            return this.keyItems.Length.CompareTo(other.keyItems.Length);
        }

        public override int GetHashCode()
        {
            int hash = 64761987;
            if (this.keyItems != null)
            {
                hash ^= this.keyItems.Length.GetHashCode();
                for (int it = 0; it < this.keyItems.Length; it++)
                    hash ^= this.keyItems[it].GetHashCode();
            }

            return hash;
        }

        public int this[int index]
        {
            get { return this.keyItems[index]; }
        }

        public override bool Equals(object obj)
        {
            return obj is AlternativeKey && this.Equals((AlternativeKey)obj);
        }

        public int Length
        {
            get { return this.keyItems == null ? 0 : this.keyItems.Length; }
        }
    }
}
