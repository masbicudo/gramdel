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
        private ProductionState state;
        private ProductAlternative[] alternatives;
        private int successCount;
        private int failureCount;
        private int capacity;

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

        /// <summary>
        /// Ensures the execution of the parsing method (also known as producer) at the position of this production.
        /// </summary>
        /// <param name="context"></param>
        public void Execute(ParsingLocalContext context)
        {
            if (this.state == ProductionState.Created)
            {
                this.state = ProductionState.Started;
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

        /// <summary>
        /// Gets a value indicating whether production alternatives have all been analysed.
        /// </summary>
        public bool IsClosed
        {
            get { return this.state == ProductionState.Closed; }
        }

        /// <summary>
        /// Closes the production of this producer.
        /// No more alternatives will be produced after closing the production.
        /// </summary>
        /// <param name="context"></param>
        public void CloseProduction(ParsingLocalContext context)
        {
            if (this.IsClosed)
                throw new Exception("Production is already closed.");

            this.state = ProductionState.Closed;
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

        public bool TryGetAlternative(int alternativeIndex, out object product)
        {
            if (this.alternatives[alternativeIndex].State == ProductAlternativeState.Succeded)
            {
                product = this.alternatives[alternativeIndex].Product;
                return true;
            }

            product = null;
            return false;
        }

        /// <summary>
        /// Indicates that an alternative product has been produced by a producer method.
        /// </summary>
        /// <typeparam name="TNode">Type of the produced object.</typeparam>
        /// <param name="key">Alternative key indicating the alternative slot to be filled by the given product.</param>
        /// <param name="context">Execution context for the continuations of the succeded alternative.</param>
        /// <param name="product">Product produced by the parsing method (the producer).</param>
        public void ItemProduced<TNode>(AlternativeKey key, ParsingLocalContext context, TNode product)
        {
            if (this.IsClosed)
                throw new Exception("Production is closed.");

            for (int it = 0; it < key.Length; it++)
            {
                
            }

            this.successCount++;
            this.alternatives[key[0]].State = ProductAlternativeState.Succeded;
            this.alternatives[key[0]].Product = product;

            this.ExecuteContinuations(key, product, context);
        }

        /// <summary>
        /// Executes all continuations for a given product.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="key"> </param>
        /// <param name="product"></param>
        /// <param name="context"></param>
        private void ExecuteContinuations<TNode>(AlternativeKey key, TNode product, ParsingLocalContext context)
        {
            foreach (var action in this.successContinuations.OfType<SuccessContinuation<TNode>>())
                action(key, product, context);
        }

        /// <summary>
        /// Indicates that an alternative has failed.
        /// </summary>
        /// <param name="alternativeIndex"></param>
        /// <param name="context"></param>
        public void ProductionFailed(int alternativeIndex, ParsingLocalContext context)
        {
            if (this.IsClosed)
                throw new Exception("Production is closed.");

            this.failureCount++;
            this.alternatives[alternativeIndex].State = ProductAlternativeState.Failed;
            this.alternatives[alternativeIndex].Product = null;
        }

        /// <summary>
        /// Creates slots for alternatives at any position of the alternatives list.
        /// </summary>
        /// <param name="indexOfNewSlot">Index to place the new slots at.</param>
        /// <param name="numberOfSlotsToCreate">Number of slots to create.</param>
        public void CreateAlternativeSlots(int indexOfNewSlot, int numberOfSlotsToCreate)
        {
            if (this.IsClosed)
                throw new Exception("Production is closed.");

            if (indexOfNewSlot < 0)
                throw new Exception("Cannot create alternative slots. Initial index must be greater than or equal to zero.");

            if (this.alternatives != null && this.alternatives[indexOfNewSlot].Order.IsDefined)
                throw new Exception("Cannot create alternative slots. Indexes at the given position are already consolidated.");

            if (this.alternatives != null && this.alternatives.Length < indexOfNewSlot)
                throw new Exception(string.Format("Cannot create alternative slots. Initial index is out of bounds. Maximum value is {0}.", this.alternatives.Length));

            this.capacity += numberOfSlotsToCreate;

            this.alternatives = this.alternatives ?? new ProductAlternative[Math.Max(4, this.capacity)];

            if (this.capacity > this.alternatives.Length)
                Array.Resize(ref this.alternatives, this.alternatives.Length * 2);

            var destination = indexOfNewSlot + numberOfSlotsToCreate;
            var numberOfItemsToCopy = this.failureCount + this.successCount - destination;
            Array.Copy(this.alternatives, indexOfNewSlot, this.alternatives, destination, numberOfItemsToCopy);
            Array.Clear(this.alternatives, indexOfNewSlot, numberOfSlotsToCreate);
        }

        internal void ItemProduced(int p, ParsingLocalContext ctx, object gramdelNode)
        {
            throw new NotImplementedException();
        }
    }
}
