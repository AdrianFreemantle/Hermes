using System;

using Microsoft.Practices.ServiceLocation;

namespace Hermes
{
    public interface IObjectBuilder : IServiceLocator, IDisposable
    {
        IObjectBuilder BeginLifetimeScope();
    }
}
