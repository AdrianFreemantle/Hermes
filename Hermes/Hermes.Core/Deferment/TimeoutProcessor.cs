using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Backoff;
using Hermes.Configuration;
using Hermes.Logging;
using Hermes.Transports;

namespace Hermes.Core.Deferment
{
    public class TimeoutProcessor : ITimeoutProcessor
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

        private void WorkerAction(object obj)
        {
            var backoff = new BackOff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
            var cancellationToken = (CancellationToken)obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                var expired = timeoutStore.GetExpired().ToList();
                bool foundWork = false;

                if (expired.Any())
                {
                    foundWork = true;

                    foreach (var id in expired)
                    {
                        TimeoutData timeoutData;

                        if (timeoutStore.TryRemove(id, out timeoutData))
                        {
                            Logger.Debug("Sending expired message: {0} to {1}", id, timeoutData.Destination);
                            messageSender.Send(timeoutData.ToMessageEnvelope(), timeoutData.Destination);
                        }
                    }
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
    }
}