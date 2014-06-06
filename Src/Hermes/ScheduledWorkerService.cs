using System;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Failover;
using Hermes.Scheduling;

namespace Hermes
{
    public abstract class ScheduledWorkerService : IAmStartable, IDisposable
    {
        private static readonly TimeSpan SixHunderedMilliseconds = TimeSpan.FromMilliseconds(600);
        private static readonly TimeSpan TenMilliseconds = TimeSpan.FromMilliseconds(10);

        private readonly object syncLock = new object();
        private readonly CronSchedule cronSchedule;
        private readonly TimeSpan timespanSchedule;
        private CircuitBreaker circuitBreaker;
        private CancellationTokenSource tokenSource;
        private bool disposed;

        public virtual bool RunImmediatelyOnStartup { get; set; }

        protected abstract void DoWork();

        protected ScheduledWorkerService(CronSchedule cronSchedule)
        {
            Mandate.ParameterNotNull(cronSchedule, "cronSchedule");
            this.cronSchedule = cronSchedule;
        }

        protected ScheduledWorkerService(TimeSpan timespanSchedule)
        {
            Mandate.ParameterNotDefaut(timespanSchedule, "timespanSchedule");
            this.timespanSchedule = timespanSchedule;
        }

        ~ScheduledWorkerService()
        {
            Dispose(false);
        }               

        public void Start()
        {
            if(disposed)
                throw new ObjectDisposedException(String.Format("Unable to start service {0} as it is disposed", GetType().Name));

            lock (syncLock)
            {
                if (tokenSource == null || tokenSource.IsCancellationRequested)
                {
                    tokenSource = new CancellationTokenSource();
                    circuitBreaker = IntializeCircuitBreaker();                    
                    StartThread();
                }    
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
            CriticalError.Raise(String.Format("Fatal error in scheduled worker service {0}.", GetType().Name), ex);
        }

        protected virtual CircuitBreaker IntializeCircuitBreaker()
        {
            return new CircuitBreaker(2, TimeSpan.FromSeconds(10));
        }

        public void Stop()
        {
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
                    DoWork();
                    nextRunTime = GetNextOccurrence();
                }

                Sleep();
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

        private void Sleep()
        {
            if (cronSchedule != null || timespanSchedule > SixHunderedMilliseconds)
            {
                Thread.Sleep(SixHunderedMilliseconds); //minimum granularity for cron is one second, so we sleep just longer than half that time to avoid triggering twice in a schedule.
            }
            else if (timespanSchedule > TenMilliseconds) 
            {
                Thread.Sleep(TenMilliseconds);
            }
            else
            {
                Thread.Sleep(timespanSchedule);
            }
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