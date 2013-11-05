namespace Gramdel.Core
{
    /// <summary>
    /// Action that represents a parsing method, also known as producer.
    /// </summary>
    /// <param name="context">Local parsing context containing information and services.</param>
    public delegate void ReaderAction(ParsingLocalContext context);
}
