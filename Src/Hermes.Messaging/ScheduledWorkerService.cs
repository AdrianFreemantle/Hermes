using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Failover;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
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

        protected string GetServiceName()
        {
            return GetType().Name.SplitCamelCase();
        }

        public virtual void Start()
        {
            Logger.Info("Starting {0}", GetServiceName());

            if(disposed)
                throw new ObjectDisposedException(String.Format("Unable to start service {0} as it is disposed", GetServiceName()));

            lock (syncLock)
            {
                if (tokenSource == null || tokenSource.IsCancellationRequested)
                {
                    tokenSource = new CancellationTokenSource();
                    StartWorkers();
                }    
            }
        }        

        private void StartWorkers()
        {
            for (int i = 0; i < WorkerThreads; i++)
            {
                WorkerTask.Start(WorkerAction, tokenSource.Token);
            }
        }

        public void Stop()
        {
            Logger.Info("Stopping {0}", GetServiceName());

            lock (syncLock)
            {
                if (tokenSource != null)
                    tokenSource.Cancel();
            }
        }

        public void WorkerAction(object obj)
        {
            var cancellationToken = (CancellationToken)obj;
            var stopwatch = new Stopwatch();

            DateTime nextRunTime = RunImmediatelyOnStartup ? DateTime.Now : GetNextOccurrence();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (nextRunTime <= DateTime.Now)
                {
                    nextRunTime = GetNextOccurrence();
                    stopwatch.Start();

                    DoWork();

                    stopwatch.Stop();
                    Logger.Debug("DoWork executed in {0}", stopwatch.Elapsed.ToString());
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