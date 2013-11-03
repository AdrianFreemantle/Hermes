using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

using Hermes.Backoff;
using Hermes.Logging;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Timeouts
{
    public class TimeoutProcessor : IAmStartable
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(TimeoutProcessor)); 

        private CancellationTokenSource tokenSource;
        private readonly IPersistTimeouts timeoutStore;
        private readonly ISendMessages messageSender;

        public TimeoutProcessor(IPersistTimeouts timeoutStore, ISendMessages messageSender)
        {
            this.timeoutStore = timeoutStore;
            this.messageSender = messageSender;
        }

        public void Start()
        {
            tokenSource = new CancellationTokenSource();

            if (Settings.SecondLevelRetryAttempts > 0)
            {
                Logger.Verbose("Starting Timeout Processor");
                CancellationToken token = tokenSource.Token;
                Task.Factory.StartNew(WorkerAction, token, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            else
            {
                Logger.Verbose("Skipping starting of Timeout Processor as no second level retries are configured.");
            }
        }

        public void Stop()
        {
            Logger.Verbose("Stopping Timeout Processor");
            tokenSource.Cancel();
        }

        private void WorkerAction(object obj)
        {
            var backoff = new BackOff(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(5));
            var cancellationToken = (CancellationToken)obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                Logger.Debug("Checking for next timeout");

                if (TryProcessNextTimeout())
                {
                    backoff.Reset();
                }
                else
                {
                    backoff.Delay();
                }
            }
        }

        private bool TryProcessNextTimeout()
        {
            using (var scope = StartTransactionScope())
            {
                var result = ProcessNextTimeout();
                scope.Complete();
                return result;
            }
        }

        private bool ProcessNextTimeout()
        {
            TimeoutData timeoutData;
            if (timeoutStore.TryFetchNextTimeout(out timeoutData))
            {
                Logger.Debug("Sending expired message: {0} to {1}", timeoutData.MessageId, timeoutData.DestinationAddress);
                messageSender.Send(timeoutData.ToMessageEnvelope(), Address.Parse(timeoutData.DestinationAddress));
                return true;
            }

            return false;
        }

        private static TransactionScope StartTransactionScope()
        {
            return Settings.UseDistributedTransaction
                ? TransactionScopeUtils.Begin(TransactionScopeOption.Required)
                : TransactionScopeUtils.Begin(TransactionScopeOption.Suppress);
        }
    }
}