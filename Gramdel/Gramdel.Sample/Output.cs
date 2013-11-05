namespace Gramdel.Sample
{
    public abstract class Output
    {
        public abstract void Message(object result);

        class VoidOutput : Output
        {
            public override void Message(object result)
            {
            }
        }

        public static readonly Output Void = new VoidOutput();
    }
}