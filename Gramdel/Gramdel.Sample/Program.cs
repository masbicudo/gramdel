using Gramdel.Core;

namespace Gramdel.Sample
{
    public class Program
    {
        public virtual int Run(string[] args, Output output)
        {
            var parser = this.CreateParser();
            var code = @"
rule Main -> A;
rule A -> A 'a' | 'a' | A 'b' | 'b';
";
            var context = new ParsingLocalContext(parser.CreateGlobalContext(code), 0, new TextReader());

            var waiter = context.WaitFor(parser.Gramdel);
            waiter.ContinueWith<GramdelParser.GramdelNode>((node, ctx) => output.Message(node));
            context.Execute(parser.Gramdel);

            return 0;
        }

        /// <summary>
        /// Creates a new gramdel parser object.
        /// </summary>
        /// <returns>Gramdel parser object.</returns>
        public virtual GramdelParser CreateParser()
        {
            return new GramdelParser();
        }
    }
}
