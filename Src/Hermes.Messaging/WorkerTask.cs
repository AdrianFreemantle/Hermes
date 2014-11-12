using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Hermes.Failover;

namespace Hermes.Messaging
{
    public static class WorkerTask
    {
        public static Task Start(Action<object> workerAction, CancellationToken token)
        {
            return Task.Factory
                .StartNew(workerAction, token, token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(t =>
                {
                    t.Exception.Handle(ex =>
                    {
                        SytemCircuitBreaker.Trigger(ex);

                        if (ex is TransactionAbortedException || ex is TransactionInDoubtException)
                        {
                            CriticalError.Raise("Distributed Transaction Error", ex);
                            return false;
                        }

                        return true;
                    });

                    Start(workerAction, token);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}