
namespace Gramdel.Core
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}
