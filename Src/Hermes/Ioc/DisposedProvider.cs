using System;

namespace Hermes.Ioc
{
    public class DisposedProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            throw new ObjectDisposedException("Unable to resolve service {0} as the service provider is currently disposed.");
        }
    }
}