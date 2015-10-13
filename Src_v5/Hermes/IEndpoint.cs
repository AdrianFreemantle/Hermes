using System;
using System.Threading;

namespace Hermes
{
    public interface IEndpoint : IDisposable
    {
        void Start(CancellationToken token);
    }
}