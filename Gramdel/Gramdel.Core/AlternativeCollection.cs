using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gramdel.Core
{
    public class AlternativeCollection
    {
        private ProductAlternative[] alternatives;
        private int successCount;
        private int failureCount;
        private int capacity;

        /// <summary>
        /// Gets or sets the number of alternatives that this production can produce.
        /// </summary>
        public int AlternativesCapacity
        {
            get { return this.capacity; }
        }

        public void SetAlternativeProductsCapacity(int value)
        {
            if (value <= 0)
                throw new Exception("Must set the capacity to a value greater than zero.");

            if (value < this.capacity)
                throw new Exception("Cannot decrease the capacity.");

            this.capacity = value;

            this.alternatives = this.alternatives ?? new ProductAlternative[Math.Max(4, value)];

            if (value > this.alternatives.Length)
                Array.Resize(ref this.alternatives, this.alternatives.Length * 2);
        }

        /// <summary>
        /// Gets or sets the number of produced alternatives.
        /// </summary>
        public int AlternativeSuccessCount
        {
            get { return this.successCount; }
        }

        /// <summary>
        /// Gets or sets the number of failed alternatives, that did not become products.
        /// </summary>
        public int AlternativeFailureCount
        {
            get { return this.failureCount; }
        }
    }
}
