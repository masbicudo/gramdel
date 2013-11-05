using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gramdel.Core
{
    [DebuggerDisplay("{Description}")]
    public class Waiter
    {
        public delegate void Continuation<TNode>(TNode node, ParsingLocalContext context);

        private Dictionary<Type, object> dic = new Dictionary<Type, object>();

        internal Waiter(PositionalKey<ReaderAction> posKey)
        {
            this.PositionalKey = posKey;
        }

        internal PositionalKey<ReaderAction> PositionalKey { get; set; }

        /// <summary>
        /// Registers a continuation for this waiter.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="action"></param>
        public void ContinueWith<TNode>(Continuation<TNode> action)
        {
            object obj;
            List<Continuation<TNode>> list;
            if (this.dic.TryGetValue(typeof(TNode), out obj))
            {
                list = (List<Continuation<TNode>>)obj;
            }
            else
            {
                list = new List<Continuation<TNode>>();
                this.dic.Add(typeof(TNode), list);
            }

            list.Add(action);
        }

        internal void ExecuteContinuations<TNode>(TNode product, ParsingLocalContext context)
        {
            object obj;
            if (this.dic.TryGetValue(typeof(TNode), out obj))
            {
                var list = (List<Continuation<TNode>>)obj;
                if (list != null)
                {
                    foreach (var action in list)
                    {
                        action(product, context);
                    }
                }
            }
        }

        public string Description
        {
            get { return string.Format("{1} => {0} continuations", this.dic.Count, this.PositionalKey); }
        }
    }
}
