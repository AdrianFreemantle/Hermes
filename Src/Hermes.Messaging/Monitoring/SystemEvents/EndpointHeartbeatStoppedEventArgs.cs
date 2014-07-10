using System;

namespace Hermes.Messaging.Monitoring.SystemEvents
{
    public class EndpointHeartbeatStoppedEventArgs : EventArgs
    {
        public string Endpoint { get; protected set; }
        public DateTime LastSeen { get; protected set; }

        public EndpointHeartbeatStoppedEventArgs(string endpoint, DateTime lastSeen)
        {
            Endpoint = endpoint;
            LastSeen = lastSeen;
        }
    }
}
