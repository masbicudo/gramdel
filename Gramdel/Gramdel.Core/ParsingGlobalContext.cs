using System.Collections.Generic;

namespace Gramdel.Core
{
    /// <summary>
    /// Global context containing data for all the parsing process.
    /// </summary>
    public class ParsingGlobalContext
    {
        public ParsingGlobalContext(string code)
        {
            this.Code = code;
            this.PositionalData = new PosData[code.Length];
        }

        public struct PosData
        {
        }

        public string Code { get; set; }


        static class PositionalKey
        {
            public static PositionalKey<TKey> Create<TKey>(int position, TKey key)
            {
                return new PositionalKey<TKey>(position, key);
            }
        }

        class PositionalKeyComparer<TKey> : IEqualityComparer<PositionalKey<TKey>>
        {
            public bool Equals(PositionalKey<TKey> x, PositionalKey<TKey> y)
            {
                return x.position == y.position
                       && EqualityComparer<TKey>.Default.Equals(x.key, y.key);
            }

            public int GetHashCode(PositionalKey<TKey> obj)
            {
                return obj.position.GetHashCode() ^ EqualityComparer<TKey>.Default.GetHashCode(obj.key);
            }
        }

        class WaiterDictionary : Dictionary<PositionalKey<ReaderAction>, Waiter>
        {
            public WaiterDictionary()
                : base(new PositionalKeyComparer<ReaderAction>())
            {
            }
        }

        WaiterDictionary waiters = new WaiterDictionary();

        public Waiter WaitFor(ReaderAction action, int position)
        {
            var posKey = PositionalKey.Create(position, action);
            Waiter waiter;
            if (!this.waiters.TryGetValue(posKey, out waiter))
            {
                waiter = new Waiter(posKey);
                waiters.Add(posKey, waiter);
            }

            return waiter;
        }

        public void ItemProduced<TNode>(ReaderAction action, int origin, int alternative, ParsingLocalContext context, TNode product)
        {
            var posKey = PositionalKey.Create(origin, action);
            Waiter waiter;
            if (this.waiters.TryGetValue(posKey, out waiter))
            {
                waiter.ExecuteContinuations(product, context);
            }
        }

        public PosData[] PositionalData { get; set; }

        class ExecutorSet : HashSet<PositionalKey<ReaderAction>>
        {
            public ExecutorSet()
                : base(new PositionalKeyComparer<ReaderAction>())
            {
            }
        }

        ExecutorSet executors = new ExecutorSet();

        public void Execute(ReaderAction action, ParsingLocalContext context)
        {
            var posKey = PositionalKey.Create(context.Position, action);
            if (!executors.Contains(posKey))
            {
                executors.Add(posKey);
                action(context);
            }
        }
    }
}