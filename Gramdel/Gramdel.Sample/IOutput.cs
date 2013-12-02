namespace Gramdel.Sample
{
    public interface IOutput
    {
        void Message(object result);
        void Save(string fileName, string text);
    }
}