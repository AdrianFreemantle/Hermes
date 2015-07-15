using System;
using System.Threading;
using System.Transactions;

using Hermes.Backoff;
using Hermes.Logging;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Transports
{  
    public class PollingReceiver : IReceiveMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (PollingReceiver));

        private CancellationTokenSource tokenSource;
        private readonly IDequeueMessages dequeueStrategy;
        private Action<TransportMessage> messageReceived;

        public PollingReceiver(IDequeueMessages dequeueStrategy)
        {
            this.dequeueStrategy = dequeueStrategy;
        }

        public void Start(Action<TransportMessage> handleMessage)
        {
            messageReceived = handleMessage;
            tokenSource = new CancellationTokenSource();

            for (int i = 0; i < Settings.NumberOfWorkers; i++)
            {
                WorkerTaskFactory.Start(WorkerAction, tokenSource.Token);
            }
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
                var result = TryDequeueWork();
                scope.Complete();
                return result;
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
