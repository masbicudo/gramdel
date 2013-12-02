namespace Gramdel.Core
{
    /// <summary>
    /// Represents a single product alternative, produced by a production process that can produce multiple alternatives.
    /// </summary>
    public struct ProductAlternative
    {
        /// <summary>
        /// Field indicating the state of the production alternative.
        /// </summary>
        public ProductAlternativeState State;

        /// <summary>
        /// Field indicating the final ordering of this alternative.
        /// The value 0 indicates that no order has been assigned.
        /// </summary>
        public Ordinal Order;

        /// <summary>
        /// Filed containing the object produced by the execution of an alternative analysis.
        /// </summary>
        public object Product;

        /// <summary>
        /// Field indicating the length of this alternative in the source code.
        /// </summary>
        public int Length;
    }
}
