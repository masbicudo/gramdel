using Gramdel.Core;

namespace Gramdel.Sample
{
    public class Program
    {
        public virtual int Run(string[] args, BasicOutput output)
        {
            var parser = this.CreateParser();
            var code = @"
rule Main -> A;
rule A -> A 'a' | 'a' | A 'b' | 'b';
";
            var context = new ParsingLocalContext(new ParsingGlobalContext(code), 0, new TextReader());

            var prod = context.GetOrCreateProduction(parser.Gramdel);
            prod.ContinueWith<GramdelParser.GramdelNode>((key, node, ctx) =>
                {
                    output.Message(node);
                    var compiler = new GramdelCSharpCompiler();
                    var csharpCode = compiler.Compile(node);
                    output.Save(@"GramdelOutput\compiled.cs", csharpCode);
                });
            prod.Execute(context);

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
