using System;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Ioc
{
    public interface IContainer : IServiceLocator, IDisposable
    {
        IContainer BeginLifetimeScope();
    }
}