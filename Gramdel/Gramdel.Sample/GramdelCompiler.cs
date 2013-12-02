using Gramdel.Core;

namespace Gramdel.Sample
{
    public abstract class GramdelCompiler
    {
        public abstract string Compile(GramdelParser.GramdelNode node);
    }
}
