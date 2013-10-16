using System;
using System.Threading;

using Hermes;
using Hermes.ServiceHost;

namespace Test
{
    public class Class1 : WorkerEndpoint<
    {
        bool disposed;

        public void Run(CancellationToken token)
        {
            Console.WriteLine(DateTime.Now);
            Thread.Sleep(1000);
            throw new Exception("boom");
        }

        ~Class1()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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
            }

            disposed = true;
        }
    }
}
