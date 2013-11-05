using System;

namespace Gramdel.Sample
{
    class Starter
    {
        static void Main(string[] args)
        {
            var program = new Program();
            Environment.ExitCode = program.Run(args, Output.Void);
        }
    }
}
