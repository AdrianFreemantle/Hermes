using System;
using System.Runtime.Serialization;
using Hermes.Messaging.Monitoring.SystemEvents;

namespace Hermes.Messaging.Monitoring.DomainEvents
{
    public interface IHermesEndpointResumed : IDomainEvent
    {
        string Endpoint { get; }
        DateTime ResumedTime { get; }
    }

    public class HermesEndpointResumed : IHermesEndpointResumed
    {
        [DataMember(Name = "")]
        public string Endpoint { get; protected set; }

        [DataMember(Name = "")]
        public DateTime ResumedTime { get; protected set; }

        public HermesEndpointResumed(EndpointHeartbeatResumedEventArgs e)
        {
            Endpoint = e.Endpoint;
            ResumedTime = e.ResumedTime;
        }
    }
}