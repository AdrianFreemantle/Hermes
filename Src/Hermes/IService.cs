using System;
using System.Threading;

namespace Hermes
{
    public interface IService : IDisposable
    {
        void Run(CancellationToken token);
    }
}