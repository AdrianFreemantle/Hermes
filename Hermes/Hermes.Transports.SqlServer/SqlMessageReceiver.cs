using System;
using System.Threading;
using System.Threading.Tasks;

using Hermes.Backoff;
using Hermes.Configuration;

namespace Hermes.Transports.SqlServer
{  
    public class SqlMessageReceiver : IDequeueMessages
    {
        private CancellationTokenSource tokenSource;
        private readonly IMessageDequeueStrategy dequeueStrategy;
        private readonly IProcessMessages messageProcessor;
        private Address address;

        public SqlMessageReceiver(IMessageDequeueStrategy dequeueStrategy, IProcessMessages messageProcessor)
        {
            this.dequeueStrategy = dequeueStrategy;
            this.messageProcessor = messageProcessor;
        }

        public void Start(Address queueAddress)
        {
            if (queueAddress == null)
            {
                throw new ArgumentNullException("queueAddress");
            }

            address = queueAddress;
            tokenSource = new CancellationTokenSource();

            for (int i = 0; i < Settings.NumberOfWorkers; i++)
            {
                StartThread();
            }
        }

        private void StartThread()
        {
            CancellationToken token = tokenSource.Token;
            Task.Factory.StartNew(WorkerAction, token, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void WorkerAction(object obj)
        {
            var backoff = new BackOff(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(1000));
            var cancellationToken = (CancellationToken)obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                bool foundWork;

                using (var scope = TransactionScopeUtils.Begin())
                {
                    var message = dequeueStrategy.Dequeue(address);
                    foundWork = ProcessMessage(message);
                    scope.Complete();
                }

                SlowDownPollingIfNoWorkAvailable(foundWork, backoff);
            }
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

        public bool ProcessMessage(MessageEnvelope message)
        {
            if (message == MessageEnvelope.Undefined)
            {
                return false;
            }

            try
            {
                messageProcessor.Process(message);
            }
            catch (MessageProcessingFailedException ex)
            {
                //todo send to second level retry queue or dead letter queue depending on retry count.
            }

            return true;
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }
    }
}
