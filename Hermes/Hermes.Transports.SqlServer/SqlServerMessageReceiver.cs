using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Transports.SqlServer
{
    public class SqlServerMessageReceiver : IDequeueMessages
    {
        private CancellationTokenSource tokenSource;
        private readonly IMessageDequeueStrategy dequeueStrategy;
        private readonly IProcessMessages messageProcessor;
        private Address address;

        const int maximumConcurrencyLevel = 4;

        public SqlServerMessageReceiver(IMessageDequeueStrategy dequeueStrategy, IProcessMessages messageProcessor)
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

            for (int i = 0; i < maximumConcurrencyLevel; i++)
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
            var cancellationToken = (CancellationToken)obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = TransactionScopeUtils.Begin())
                {
                    var message = dequeueStrategy.Dequeue(address);
                    ProcessMessage(message);
                    scope.Complete();
                }

                Thread.Sleep(50);
            }
        }

        public void ProcessMessage(MessageEnvelope message)
        {
            if (message == MessageEnvelope.Undefined)
            {
                return;
            }

            try
            {
                messageProcessor.Process(message);
            }
            catch (MessageProcessingFailedException ex)
            {
                //send to second level retry queue or dead letter queue depending on retry count.
            }
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }        
    }
}
