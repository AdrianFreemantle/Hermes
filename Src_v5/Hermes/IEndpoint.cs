using System;
using System.Threading;
using Hermes.Ioc;

namespace Hermes
{
    public interface IEndpoint : IDisposable
    {
        IEndpoint RegisterDependencies<T>() where T : IRegisterDependencies, new();
        void Start(CancellationToken token);
    }
}