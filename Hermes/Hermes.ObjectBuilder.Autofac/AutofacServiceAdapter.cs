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
        public ILifetimeScope lifetimeScope { get; set; }
        private bool disposed;

        public AutofacServiceAdapter()
        {
            
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

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

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
            return new AutofacServiceAdapter(lifetimeScope.BeginLifetimeScope());
        }
    }
}
