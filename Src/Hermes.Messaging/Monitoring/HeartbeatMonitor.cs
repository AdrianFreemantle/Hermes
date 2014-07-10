using System;
using System.Timers;
using Hermes.Logging;
using Hermes.Messaging.Monitoring.SystemEvents;

namespace Hermes.Messaging.Monitoring
{
    public class HeartbeatMonitor : IDisposable
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(HeartbeatMonitor));
        private readonly TimeSpan monitoringPeriod = TimeSpan.FromMinutes(1);
        private readonly Timer timer;
        private readonly string name;
        private bool stopped = true;
        private DateTime lastSeen = DateTime.Now;

        public event EndpointHeartbeatStoppedHandler OnEndpointHeartbeatStopped;
        public event EndpointHeartbeatResumedHandler OnEndpointHeartbeatResumed;

        public HeartbeatMonitor(string name)
        {
            this.name = name;

            timer = new Timer
            {
                Interval = monitoringPeriod.TotalMilliseconds,
                AutoReset = false,
            };

            timer.Elapsed += Elapsed;
        }

        public void Start()
        {
            Reset();
        }

        public void Reset()
        {
            timer.Stop();
            Resume();
        }

        void Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.Error("Endpoint {0} appears to be down", name);
            stopped = true;

            if (OnEndpointHeartbeatStopped != null)
                OnEndpointHeartbeatStopped(new EndpointHeartbeatStoppedEventArgs(name, lastSeen), this);
        }

        private void Resume()
        {
            if (stopped && OnEndpointHeartbeatResumed != null)
            {
                OnEndpointHeartbeatResumed(new EndpointHeartbeatResumedEventArgs(name, lastSeen), this);
            }

            timer.Start();
            lastSeen = DateTime.Now;
            stopped = false;
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
