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

        class ProductionDictionary : Dictionary<PositionalKey<ReaderAction>, ProductionContext>
        {
            public ProductionDictionary()
                : base(new PositionalKeyComparer<ReaderAction>())
            {
            }
        }

        ProductionDictionary productions = new ProductionDictionary();

        public ProductionContext GetProduction(ReaderAction action, int position)
        {
            var posKey = PositionalKey.Create(position, action);
            ProductionContext productionContext;
            if (!this.productions.TryGetValue(posKey, out productionContext))
            {
                productionContext = new ProductionContext(posKey);
                productions.Add(posKey, productionContext);
            }

            return productionContext;
        }

        public PosData[] PositionalData { get; set; }
    }
}
