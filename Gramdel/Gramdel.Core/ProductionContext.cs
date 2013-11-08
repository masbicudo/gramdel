using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gramdel.Core
{
    /// <summary>
    /// Contains information about a specific production process at a specific source code location.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    public class ProductionContext
    {
        private PositionalKey<ReaderAction> positionalKey;
        private readonly List<object> successContinuations = new List<object>();
        private readonly List<FailureContinuation> failureContinuations = new List<FailureContinuation>();
        private bool isClosed;
        private ProductAlternative[] alternatives;
        private int successCount;
        private int failureCount;

        internal ProductionContext(PositionalKey<ReaderAction> posKey)
        {
            this.positionalKey = posKey;
        }

        /// <summary>
        /// Gets the origin of this production in the source code.
        /// </summary>
        public int Origin
        {
            get { return this.positionalKey.position; }
        }

        /// <summary>
        /// Gets the parsing method associated with this production.
        /// </summary>
        public ReaderAction Action
        {
            get { return this.positionalKey.key; }
        }

        /// <summary>
        /// Registers a success continuation for this production.
        /// </summary>
        /// <typeparam name="TNode">  </typeparam>
        /// <param name="action">  </param>
        public void ContinueWith<TNode>(SuccessContinuation<TNode> action)
        {
            this.successContinuations.Add(action);
        }

        /// <summary>
        /// Registers a failure continuation for this production.
        /// </summary>
        /// <param name="action"></param>
        public void FailWith(FailureContinuation action)
        {
            this.failureContinuations.Add(action);
        }

        private bool executed;

        /// <summary>
        /// Ensures the execution of the parsing method (also known as producer) at the position of this production.
        /// </summary>
        /// <param name="context"></param>
        public void Execute(ParsingLocalContext context)
        {
            if (!this.executed)
            {
                this.executed = true;
                this.Action(context);
            }
        }

        /// <summary>
        /// Gets a text describing this object.
        /// </summary>
        protected string Description
        {
            get
            {
                int continuations = this.successContinuations.Count;
                var result = string.Format("{1} => {0} continuations", continuations, this.positionalKey);
                return result;
            }
        }

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
            if (value < this.capacity)
                throw new Exception("Cannot decrease the capacity.");

            this.capacity = value;

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

        /// <summary>
        /// Gets a value indicating whether production alternatives have all been analysed.
        /// </summary>
        public bool IsClosed
        {
            get { return this.isClosed; }
        }

        /// <summary>
        /// Closes the production of this producer.
        /// No more alternatives will be produced after closing the production.
        /// </summary>
        /// <param name="context"></param>
        public void CloseProduction(ParsingLocalContext context)
        {
            if (this.isClosed)
                throw new Exception("Production is already closed.");

            this.isClosed = true;
            Array.Resize(ref this.alternatives, this.capacity);

            context.Position = this.Origin;
            if (this.successCount == 0)
                this.ExecuteFailureEvents(context);
        }

        /// <summary>
        /// Executes all failure events.
        /// This should be called when all alternatives in a production process fails.
        /// </summary>
        /// <param name="context"></param>
        private void ExecuteFailureEvents(ParsingLocalContext context)
        {
            foreach (var action in this.failureContinuations)
                action(context);
        }

        /// <summary>
        /// Gets the collection of alternatives that may be analsysed by the production process.
        /// </summary>
        /// <remarks>The returned array is a copy of the alternatives produced internally.</remarks>
        public ProductAlternative[] Alternatives
        {
            get { return this.alternatives.ToArray(); }
        }

        public bool TryGetAlternative(int alternative, out object product)
        {
            if (this.alternatives[alternative].State == ProductAlternativeState.Succeded)
            {
                product = this.alternatives[alternative].Product;
                return true;
            }

            product = null;
            return false;
        }

        /// <summary>
        /// Indicates that an alternative product has been produced by this producer method.
        /// </summary>
        /// <typeparam name="TNode">Type of the produced object.</typeparam>
        /// <param name="alternative">Alternative ID that must be filled with this product.</param>
        /// <param name="context">Execution context for the continuations of the succeded alternative.</param>
        /// <param name="product">Product produced by the parsing method (the producer).</param>
        public void ItemProduced<TNode>(int alternative, ParsingLocalContext context, TNode product)
        {
            if (this.isClosed)
                throw new Exception("Production is closed.");

            this.successCount++;
            this.alternatives[alternative].State = ProductAlternativeState.Succeded;
            this.alternatives[alternative].Product = product;

            this.ExecuteContinuations(product, context);
        }

        /// <summary>
        /// Executes all continuations for a given product.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="product"></param>
        /// <param name="context"></param>
        private void ExecuteContinuations<TNode>(TNode product, ParsingLocalContext context)
        {
            foreach (var action in this.successContinuations.OfType<SuccessContinuation<TNode>>())
                action(product, context);
        }

        /// <summary>
        /// Indicates that an alternative has failed.
        /// </summary>
        /// <param name="alternative"></param>
        /// <param name="context"></param>
        public void ProductionFailed(int alternative, ParsingLocalContext context)
        {
            if (this.isClosed)
                throw new Exception("Production is closed.");

            this.failureCount++;
            this.alternatives[alternative].State = ProductAlternativeState.Failed;
            this.alternatives[alternative].Product = null;
        }
    }
}
