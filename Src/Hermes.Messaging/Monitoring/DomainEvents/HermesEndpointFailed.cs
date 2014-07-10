using System;
using System.Runtime.Serialization;
using Hermes.Messaging.Monitoring.SystemEvents;

namespace Hermes.Messaging.Monitoring.DomainEvents
{
    public interface IHermesEndpointFailed : IDomainEvent
    {
        string Endpoint { get; }
        DateTime LastSeen { get; }
    }

    [DataContract]
    public class HermesEndpointFailed : IHermesEndpointFailed
    {
        [DataMember(Name = "")]
        public string Endpoint { get; protected set; }

        [DataMember(Name = "")]
        public DateTime LastSeen { get; protected set; }

        public HermesEndpointFailed(EndpointHeartbeatStoppedEventArgs e)
        {
            Endpoint = e.Endpoint;
            LastSeen = e.LastSeen;
        }
    }
}