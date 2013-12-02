namespace Gramdel.Core
{
    public delegate void SuccessContinuation<in TNode>(AlternativeKey key, TNode node, ParsingLocalContext context);
    public delegate void FailureContinuation(ParsingLocalContext context);

    public class FinalIndexContinuator<TNode> : IContinuator<TNode>
    {
        public void InsertProduct(AlternativeKey key, TNode node, ParsingLocalContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
