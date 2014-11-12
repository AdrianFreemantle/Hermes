using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

using Hermes.Backoff;
using Hermes.Failover;
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
            if(Settings.IsSendOnly)
                return;

            PurgeQueueIfRequired();

            Logger.Info("Starting Timeout Processor");

            tokenSource = new CancellationTokenSource();
            WorkerTask.Start(WorkerAction, tokenSource.Token);
        }

        private void PurgeQueueIfRequired()
        {
            if (Settings.FlushQueueOnStartup)
            {
                timeoutStore.Purge();
            }
        }

        public void Stop()
        {
            Logger.Info("Stopping Timeout Processor");

            if(tokenSource != null)
                tokenSource.Cancel();
        }

        private void WorkerAction(object obj)
        {
            var backoff = new BackOff(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(5));
            var cancellationToken = (CancellationToken)obj;

            while (!cancellationToken.IsCancellationRequested)
            {
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
            ITimeoutData timeoutData;

            if (timeoutStore.TryFetchNextTimeout(out timeoutData))
            {
                Logger.Debug("Sending expired message: {0} to {1}", timeoutData.MessageId, timeoutData.DestinationAddress);

                var transportMessage = TimeoutData.ToTransportmessage(timeoutData);
                transportMessage.Headers[HeaderKeys.SentTime] = DateTime.UtcNow.ToWireFormattedString();
                messageSender.Send(transportMessage, Address.Parse(timeoutData.DestinationAddress));
                return true;
            }

            return false;
        }

        private static TransactionScope StartTransactionScope()
        {
            return Settings.DisableDistributedTransactions
                ? TransactionScopeUtils.Begin(TransactionScopeOption.Suppress)
                : TransactionScopeUtils.Begin(TransactionScopeOption.Required);
        }
    }
}