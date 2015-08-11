using System;
using System.Security.Permissions;
using System.Threading;
using System.Web;

namespace Hermes.Ioc
{
    [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true, Synchronization = true)]
    public class WebSafeThreadLocal<T> : IDisposable
    {
        private readonly ThreadLocal<T> threadLocal = new ThreadLocal<T>();
        private readonly Guid key = Guid.NewGuid();
        private bool disposed;

        public T Value
        {
            get { return GetValue(); }
            set { SetValue(value);}
        }

        public bool IsValueCreated
        {
            get { return HasValue(); }
        }

        public void SetValue(T value)
        {
            if (HttpContext.Current == null)
            {
                threadLocal.Value = value;
                return;
            }

            HttpContext.Current.Items[key] = value;
        }        

        private T GetValue()
        {
            if (HttpContext.Current == null)
            {
                return threadLocal.Value;
            }

            if(HttpContext.Current.Items.Contains(key))
                return (T)HttpContext.Current.Items[key];

            return default(T);
        }

        private bool HasValue()
        {
            if (HttpContext.Current == null)
            {
                return threadLocal.IsValueCreated;
            }

            return HttpContext.Current.Items.Contains(key);
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

            if (disposing)
            {
                threadLocal.Dispose();
            }

            disposed = true;
        }
    }
}