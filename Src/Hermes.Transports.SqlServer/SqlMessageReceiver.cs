using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Hermes.Backoff;
using Hermes.Configuration;
using Hermes.Ioc;
using Hermes.Logging;

namespace Hermes.Transports.SqlServer
{  
    public class SqlMessageReceiver : IDequeueMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SqlMessageReceiver));

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

        public void Stop()
        {
            tokenSource.Cancel();
        }

        public void WorkerAction(object obj)
        {
            var backoff = new BackOff(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(1000));
            var cancellationToken = (CancellationToken)obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                TryDequeueWork(backoff);
            }
        }

        private void TryDequeueWork(BackOff backoff)
        {
            bool foundWork = false;

            try
            {
                foundWork = DequeueWork();
            }
            catch (TransactionAbortedException)
            {
                Logger.Info("Transaction Aborted Exception.");
            }
            catch (Exception ex)
            {
                Logger.Info("Error while attempting to dequeue work: {0}", ex.Message);
            }
            finally
            {
                SlowDownPollingIfNoWorkAvailable(foundWork, backoff);
            }
        }

        private bool DequeueWork()
        {
            using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.Required))
            {
                var message = dequeueStrategy.Dequeue(address);

                try
                {
                    return ProcessMessage(message);
                }
                finally
                {
                    scope.Complete();
                }
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
            try
            {
                if (message == MessageEnvelope.Undefined)
                {
                    return false;
                }

                messageProcessor.Process(message);
            }
            catch (MessageProcessingFailedException ex)
            {
                Logger.Info("Moving message {0} to dead letter or retry queue.", message.MessageId);
                //todo send to second level retry queue or dead letter queue depending on retry count.
            }

            return true;
        }      
    }
}
