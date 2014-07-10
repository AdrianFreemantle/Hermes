using System;
using System.Collections.Generic;
using Hermes.Messaging.Monitoring.SystemEvents;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;
using Hermes.Pipes;

namespace Hermes.Messaging.Monitoring.Pipeline
{
    public class HeartbeatMonitorModule : IModule<IncomingMessageContext>
    {
        private static readonly Dictionary<string, HeartbeatMonitor> HeartBeats = new Dictionary<string, HeartbeatMonitor>();
        private static readonly object SyncLock = new object();
        
        public static event EndpointHeartbeatStoppedHandler OnEndpointHeartbeatStopped;
        public static event EndpointHeartbeatResumedHandler OnEndpointHeartbeatResumed;

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            HeaderValue endpoint;

            input.TryGetHeaderValue(HeaderKeys.Heartbeat, out endpoint);

            if (endpoint != null)
            {
                UpdateHeartbeat(endpoint);
                return true;
            }

            return next();
        }

        private void UpdateHeartbeat(HeaderValue endpoint)
        {
            lock (SyncLock)
            {
                if (HeartBeats.ContainsKey(endpoint.Value))
                {
                    HeartBeats[endpoint.Value].Reset();
                }
                else
                {
                    var monitor = new HeartbeatMonitor(endpoint.Value);
                    monitor.OnEndpointHeartbeatStopped += OnHeartbeatStopped;
                    monitor.OnEndpointHeartbeatResumed += OnHeartbeatResumed;
                    HeartBeats[endpoint.Value] = monitor;
                    monitor.Start();
                }
            }
        }

        void OnHeartbeatResumed(EndpointHeartbeatResumedEventArgs e, object sender)
        {
            if (OnEndpointHeartbeatResumed != null)
                OnEndpointHeartbeatResumed(e, sender);
        }

        private void OnHeartbeatStopped(EndpointHeartbeatStoppedEventArgs e, object sender)
        {
            if (OnEndpointHeartbeatStopped != null)
                OnEndpointHeartbeatStopped(e, sender);
        }
    }
}
