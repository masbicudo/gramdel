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
        /// Filed containing the object produced by the execution of an alternative analysis.
        /// </summary>
        public object Product;
    }
}
