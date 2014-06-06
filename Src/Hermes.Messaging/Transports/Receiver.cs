using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

using Hermes.Backoff;
using Hermes.Failover;
using Hermes.Logging;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Transports
{  
    public class Receiver : IReceiveMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (Receiver));
        private readonly CircuitBreaker circuitBreaker = new CircuitBreaker(Settings.CircuitBreakerThreshold, Settings.CircuitBreakerReset);

        private CancellationTokenSource tokenSource;
        private readonly IDequeueMessages dequeueStrategy;
        private Action<TransportMessage> messageReceived;

        public Receiver(IDequeueMessages dequeueStrategy)
        {
            this.dequeueStrategy = dequeueStrategy;
        }

        public void Start(Action<TransportMessage> handleMessage)
        {
            messageReceived = handleMessage;
            tokenSource = new CancellationTokenSource();

            for (int i = 0; i < Settings.NumberOfWorkers; i++)
            {
                StartThread();
            }
        }

        private void StartThread()
        {
            CancellationToken token = tokenSource.Token;

            Task.Factory
                .StartNew(WorkerAction, token, token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(t =>
                {
                    t.Exception.Handle(ex =>
                    {
                        circuitBreaker.Execute(() => CriticalError.Raise("Fatal error while attempting to dequeue messages.", ex));
                        return true;
                    });

                    StartThread();
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Stop()
        {
            if(tokenSource != null)
                tokenSource.Cancel();
        }

        public void WorkerAction(object obj)
        {
            var backoff = new BackOff(TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1000));
            var cancellationToken = (CancellationToken)obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                bool foundWork = DequeueWork();
                SlowDownPollingIfNoWorkAvailable(foundWork, backoff);
            }
        }
        
        private bool DequeueWork()
        {
            using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.Required))
            {
                try
                {
                    return TryDequeueWork();
                }
                finally
                {
                    Logger.Debug("Commiting transaction scope");
                    scope.Complete();
                }
            }
        }

        private bool TryDequeueWork()
        {
            TransportMessage transportMessage = dequeueStrategy.Dequeue();

            if (transportMessage == TransportMessage.Undefined)
            {
                return false;
            }

            messageReceived(transportMessage);
            return true;
        }

        private static void SlowDownPollingIfNoWorkAvailable(bool foundWork, BackOff backoff)
        {
            if (foundWork)
            {
                backoff.Reset();
            }
            else
            {
                backoff.Delay();
            }
        }        
    }
}
