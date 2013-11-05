using System;
using System.Diagnostics;

namespace Gramdel.Core
{
    /// <summary>
    /// Context structure that must be provided for each parsing method being called.
    /// This contains data that must remain independent for each method in the chain of calls.
    /// </summary>
    [DebuggerDisplay("{Preview}")]
    public struct ParsingLocalContext
    {
        /// <summary>
        /// Gets or sets the current parsing position.
        /// </summary>
        public int Position;

        /// <summary>
        /// Gets or sets the text reader in this structure.
        /// </summary>
        public TextReader Reader;

        /// <summary>
        /// The global parsing context containing information about the overall process of parsing.
        /// </summary>
        private readonly ParsingGlobalContext globalContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingLocalContext"/> struct.
        /// </summary>
        /// <param name="globalContext"> The global parsing context. </param>
        /// <param name="position"> Current parsing position inside the source code. </param>
        /// <param name="reader"> The text reader used to read the code. </param>
        public ParsingLocalContext(ParsingGlobalContext globalContext, int position, TextReader reader)
        {
            this.globalContext = globalContext;
            this.Position = position;
            this.Reader = reader;
        }

        /// <summary>
        /// Gets the global parsing context.
        /// </summary>
        public ParsingGlobalContext GlobalContext
        {
            get { return this.globalContext; }
        }

        /// <summary>
        /// Gets the preview of the current parsing location in the code.
        /// </summary>
        public string Preview
        {
            get
            {
                var start = Math.Max(this.Position - 10, 0);
                var end = Math.Min(this.Position + 20, this.GlobalContext.Code.Length);

                return (start == 0 ? "[^]" : "[…]")
                       + this.GlobalContext.Code.Substring(start, this.Position - start)
                       + "[●]"
                       + this.GlobalContext.Code.Substring(this.Position, end - this.Position)
                       + (end == this.GlobalContext.Code.Length ? "[$]" : "[…]");
            }
        }

        /// <summary>
        /// Gets the code that is being parsed.
        /// </summary>
        public string Code
        {
            get { return this.GlobalContext.Code; }
        }

        /// <summary>
        /// Waits for a parsing product in the current position of the source code.
        /// </summary>
        /// <param name="action">Action representing the producer, that is the parsing method.</param>
        /// <returns>Waiter object that can be used to create continuations of the parsing process.</returns>
        public Waiter WaitFor(ReaderAction action)
        {
            return this.GlobalContext.WaitFor(action, this.Position);
        }

        /// <summary>
        /// Indicated that a product has been produced, by the use of a parsing method (also called producer).
        /// </summary>
        /// <typeparam name="TNode">Type of the produced object.</typeparam>
        /// <param name="action">Action representing the producer, that is the parsing method.</param>
        /// <param name="alternative">Indicates the alternative slot in which to place the product.</param>
        /// <param name="origin">Product origin in the source code.</param>
        /// <param name="node">Produced object.</param>
        public void ItemProduced<TNode>(ReaderAction action, int alternative, int origin, TNode node)
        {
            this.GlobalContext.ItemProduced(action, origin, alternative, this, node);
        }

        /// <summary>
        /// Ensures the execution of a parsing method (also known as producer)
        /// at the current position in the source code.
        /// </summary>
        /// <param name="action">Action representing the producer, that is the parsing method.</param>
        public void Execute(ReaderAction action)
        {
            this.GlobalContext.Execute(action, this);
        }

        /// <summary>
        /// Indicates that an alternative has failed.
        /// </summary>
        /// <param name="action"> The action. </param>
        /// <param name="alternative"> The alternative number. </param>
        public void Fail(ReaderAction action, int alternative)
        {
        }

        public void SetAlternativesCount(ReaderAction action, int p)
        {
        }
    }
}