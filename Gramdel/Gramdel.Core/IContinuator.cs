namespace Gramdel.Core
{
    public interface IContinuator<in TNode>
    {
        void InsertProduct(AlternativeKey key, TNode node, ParsingLocalContext context);
    }
}
