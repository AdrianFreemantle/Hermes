using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autofac;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.ObjectBuilder.Autofac
{   
    public class AutofacServiceAdapter : ServiceLocatorImplBase, IObjectBuilder
    {
        private readonly ContainerBuilder builder;
        private ILifetimeScope lifetimeScope;
        private bool disposed;

        public AutofacServiceAdapter(ContainerBuilder builder)
        {
            this.builder = builder;
        }

        public AutofacServiceAdapter(ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null)
            {
                throw new ArgumentNullException("lifetimeScope");
            }

            this.lifetimeScope = lifetimeScope;
        }

        ~AutofacServiceAdapter()
        {
            Dispose(false);
        }

        private void BuildContiner()
        {
            if (builder != null && lifetimeScope == null)
            {
                lifetimeScope = builder.Build();
            }
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            BuildContiner();

            return key != null
                ? lifetimeScope.ResolveNamed(key, serviceType)
                : lifetimeScope.Resolve(serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            BuildContiner();

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            object instance = lifetimeScope.Resolve(enumerableType);

            return ((IEnumerable)instance).Cast<object>();
        }        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && lifetimeScope != null)
            {
                lifetimeScope.Dispose();
            }

            disposed = true;
        }

        public IObjectBuilder BeginLifetimeScope()
        {
            BuildContiner();
            return new AutofacServiceAdapter(lifetimeScope.BeginLifetimeScope());
        }
    }
}
