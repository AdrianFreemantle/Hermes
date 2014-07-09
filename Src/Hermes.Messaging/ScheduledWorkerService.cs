using System;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Failover;
using Hermes.Logging;
using Hermes.Scheduling;

namespace Hermes.Messaging
{
    public abstract class ScheduledWorkerService : IAmStartable, IDisposable
    {
        private readonly object syncLock = new object();
        protected readonly uint WorkerThreads = 1;

        protected readonly ILog Logger;
        protected bool RunImmediatelyOnStartup;

        private static readonly TimeSpan TenMilliseconds = TimeSpan.FromMilliseconds(10);

        private CronSchedule cronSchedule;
        private TimeSpan timespanSchedule;
        private CircuitBreaker circuitBreaker;
        private CancellationTokenSource tokenSource;
        private bool disposed;

        protected abstract void DoWork();

        protected ScheduledWorkerService()
        {      
            Logger = LogFactory.BuildLogger(GetType());
            timespanSchedule = TimeSpan.FromMinutes(1);
            RunImmediatelyOnStartup = true;
        }

        public void SetSchedule(CronSchedule schedule)
        {
            Mandate.ParameterNotNull(schedule, "schedule");

            cronSchedule = schedule;
        }

        public void SetSchedule(TimeSpan timeSpan)
        {
            if (timespanSchedule < TenMilliseconds)
            {
                Logger.Warn("A scheduled worker has a minimum allowed schedule of {0} millisecconds. The default minimum will be used.", TenMilliseconds.Milliseconds);
                timespanSchedule = TenMilliseconds;
            }

            timespanSchedule = timeSpan;
        }

        ~ScheduledWorkerService()
        {
            Dispose(false);
        }               

        public virtual void Start()
        {
            Logger.Info("Starting {0}", GetType().Name.SplitCamelCase());

            if(disposed)
                throw new ObjectDisposedException(String.Format("Unable to start service {0} as it is disposed", GetType().Name));

            lock (syncLock)
            {
                if (tokenSource == null || tokenSource.IsCancellationRequested)
                {
                    tokenSource = new CancellationTokenSource();
                    circuitBreaker = IntializeCircuitBreaker();
                    StartWorkers();
                }    
            }
        }    
    
        private void StartWorkers()
        {
            for (int i = 0; i < WorkerThreads; i++)
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
                        circuitBreaker.Execute(() => OnCircuitBreakerTriped(ex));
                        return true;
                    });

                    StartThread();
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        protected virtual void OnCircuitBreakerTriped(Exception ex)
        {
            var log = String.Format("Fatal error in scheduled worker service {0}.", GetType().Name);
            CriticalError.Raise(log, ex);
        }

        protected virtual CircuitBreaker IntializeCircuitBreaker()
        {
            return new CircuitBreaker(2, TimeSpan.FromSeconds(30));
        }

        public void Stop()
        {
            Logger.Info("Stopping {0}", GetType().Name.SplitCamelCase());

            lock (syncLock)
            {
                if (tokenSource != null)
                    tokenSource.Cancel();
            }
        }

        public void WorkerAction(object obj)
        {
            var cancellationToken = (CancellationToken)obj;

            DateTime nextRunTime = RunImmediatelyOnStartup ? DateTime.Now : GetNextOccurrence();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (nextRunTime <= DateTime.Now)
                {
                    nextRunTime = GetNextOccurrence();
                    DoWork();
                }

                Thread.Sleep(TenMilliseconds);
            }
        }

        private DateTime GetNextOccurrence()
        {
            if (cronSchedule == null)
            {
                return DateTime.Now.Add(timespanSchedule);
            }
            
            return cronSchedule.GetNextOccurrence(DateTime.Now);
        }

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
                Stop();
            }

            disposed = true;
        }
    }
}