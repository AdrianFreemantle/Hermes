using System.Threading;

namespace Hermes
{
    public interface IAmStartable
    {
        void Start(CancellationToken token);
        void Stop();
    }
}