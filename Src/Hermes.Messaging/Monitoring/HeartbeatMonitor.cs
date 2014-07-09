using System;
using System.Timers;
using Hermes.Logging;
using Hermes.Messaging.Monitoring.Events;

namespace Hermes.Messaging.Monitoring
{
    public class HeartbeatMonitor : IDisposable
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(HeartbeatMonitor));
        private readonly TimeSpan monitoringPeriod = TimeSpan.FromMinutes(1);
        private readonly Timer timer;
        private readonly string name;
        private DateTime lastSeen;

        public event EndpointHeartbeatStoppedHandler OnEndpointHeartbeatStopped;

        public HeartbeatMonitor(string name)
        {
            this.name = name;
            lastSeen = DateTime.Now;

            timer = new Timer
            {
                Interval = monitoringPeriod.TotalMilliseconds,
                AutoReset = false,
            };

            timer.Elapsed += Elapsed;
            timer.Start();
        }

        public void Reset()
        {
            timer.Stop();
            timer.Start();

            lastSeen = DateTime.Now;
            Logger.Info("Endpoint {0} heartbeat detected", name);
        }

        void Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.Error("Endpoint {0} appears to be down", name);

            if (OnEndpointHeartbeatStopped != null)
                OnEndpointHeartbeatStopped(new EndpointHeartbeatStoppedEventArgs(name, lastSeen), this);
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
