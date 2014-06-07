﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Failover;
using Hermes.Logging;
using Hermes.Scheduling;

namespace Hermes.Messaging
{
    public abstract class ScheduledWorkerService : IAmStartable, IDisposable
    {
        protected readonly ILog Logger;

        private static readonly TimeSpan TenMilliseconds = TimeSpan.FromMilliseconds(10);

        private readonly object syncLock = new object();
        private readonly CronSchedule cronSchedule;
        private readonly TimeSpan timespanSchedule;
        private CircuitBreaker circuitBreaker;
        private CancellationTokenSource tokenSource;
        private bool disposed;

        public virtual bool RunImmediatelyOnStartup { get; set; }

        protected abstract void DoWork();

        protected ScheduledWorkerService()
        {
            timespanSchedule = TimeSpan.FromSeconds(10);
        }

        protected ScheduledWorkerService(CronSchedule cronSchedule)
        {
            Mandate.ParameterNotNull(cronSchedule, "cronSchedule");
           
            Logger = LogFactory.BuildLogger(GetType());
            this.cronSchedule = cronSchedule;
        }

        protected ScheduledWorkerService(TimeSpan timespanSchedule)
        {
            Mandate.ParameterNotDefaut(timespanSchedule, "timespanSchedule");
            
            Logger = LogFactory.BuildLogger(GetType());
            
            if (timespanSchedule > TenMilliseconds)
            {
                Logger.Warn("A scheduled worker has a minimum allowed schedule of 10 millisecconds. The default minimum will be used.");
                timespanSchedule = TenMilliseconds;
            }
            
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
            var log = String.Format("Fatal error in scheduled worker service {0}.", GetType().Name);
            CriticalError.Raise(log, ex);
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
                    nextRunTime = GetNextOccurrence();
                    DoWork();
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
            if (cronSchedule == null)
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