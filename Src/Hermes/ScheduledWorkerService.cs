using System;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Failover;
using Hermes.Messaging;
using Hermes.Scheduling;

namespace Hermes
{
    public class MyTimespanScheduledWorker : ScheduledWorkerService
    {
        private readonly IInMemoryBus inMemoryBus;

        public MyTimespanScheduledWorker(IInMemoryBus inMemoryBus) 
            : base(TimeSpan.FromSeconds(30)) // run every thirty seconds
        {
            this.inMemoryBus = inMemoryBus;
        }

        protected override void DoWork()
        {
            try
            {
                //fetch data
                //execute command on inMemoryBus for each action
            }
            catch (Exception ex)
            {
                //NB only handle exceptions you can actually recover from, else throw                               
                throw;
            }
        }
    }


    public class MyCronScheduledWorker : ScheduledWorkerService
    {
        private readonly IInMemoryBus inMemoryBus;

        public MyCronScheduledWorker(IInMemoryBus inMemoryBus)
            : base(Cron.Parse("0 * 8-17 1,2,3,4,5,6,7 * Monday")) // run every thirty seconds
        {
            this.inMemoryBus = inMemoryBus;
        }

        protected override void DoWork()
        {
            try
            {
                //fetch data
                //execute command on inMemoryBus for each action
            }
            catch (Exception ex)
            {
                //NB only handle exceptions you can actually recover from, else throw                               
                throw;
            }
        }
    }

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
                        circuitBreaker.Execute(() => CriticalError.Raise(String.Format("Fatal error in scheduled worker service {0}.", GetType().Name), ex));
                        return true;
                    });

                    StartThread();
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        protected virtual CircuitBreaker IntializeCircuitBreaker()
        {
            return new CircuitBreaker(10, TimeSpan.FromSeconds(10));
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