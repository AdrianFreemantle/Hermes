using System;

namespace Hermes.Messaging.Monitoring.SystemEvents
{
    public class EndpointHeartbeatResumedEventArgs : EventArgs
    {
        public string Endpoint { get; protected set; }
        public DateTime ResumedTime { get; protected set; }

        public EndpointHeartbeatResumedEventArgs(string endpoint, DateTime resumedTime)
        {
            Endpoint = endpoint;
            ResumedTime = resumedTime;
        }
    }
}