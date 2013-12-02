using System;
using System.IO;
using System.Text;

namespace Gramdel.Sample
{
    public class BasicOutput : IOutput
    {
        private readonly string outputPath;

        public BasicOutput(string outputPath)
        {
            this.outputPath = outputPath;
        }

        public virtual void Message(object result)
        {
            Console.WriteLine(result);
        }

        public virtual void Save(string fileName, string text)
        {
            try { Directory.CreateDirectory(this.outputPath); }
            catch { }

            File.WriteAllText(Path.Combine(this.outputPath, fileName), text, Encoding.UTF8);
        }
    }
}
