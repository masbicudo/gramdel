namespace Gramdel.Core
{
    public delegate void SuccessContinuation<in TNode>(TNode node, ParsingLocalContext context);
    public delegate void FailureContinuation(ParsingLocalContext context);
}