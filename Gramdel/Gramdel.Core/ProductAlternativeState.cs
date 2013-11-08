namespace Gramdel.Core
{
    /// <summary>
    /// Enumeration consisting of states of a product alternative.
    /// </summary>
    public enum ProductAlternativeState
    {
        /// <summary>
        /// Indicates an alternative that has not yet been analysed to the end.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates an alternative that has an associated product.
        /// </summary>
        Succeded,

        /// <summary>
        /// Indicates an alternative that has failed, and as such will not have a product.
        /// </summary>
        Failed,
    }
}
