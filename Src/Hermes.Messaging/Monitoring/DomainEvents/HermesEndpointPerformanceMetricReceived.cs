using System;
using System.Runtime.Serialization;
using Hermes.Messaging.Monitoring.SystemEvents;

namespace Hermes.Messaging.Monitoring.DomainEvents
{
    public interface IHermesEndpointPerformanceMetricReceived : IDomainEvent
    {
        Address Endpoint { get; }
        int TotalMessagesProcessed { get; }
        int TotalErrorMessages { get; }
        TimeSpan AverageTimeToDeliver { get; }
        TimeSpan AverageTimeToProcess { get; }
        TimeSpan MonitoringPeriod { get; }
    }

    public class HermesEndpointPerformanceMetricReceived : IHermesEndpointPerformanceMetricReceived
    {
        [DataMember(Name = "Endpoint")]
        public Address Endpoint { get; protected set; }

        [DataMember(Name = "TotalMessagesProcessed")]
        public int TotalMessagesProcessed { get; protected set; }

        [DataMember(Name = "TotalErrorMessages")]
        public int TotalErrorMessages { get; protected set; }

        [DataMember(Name = "AverageTimeToDeliver")]
        public TimeSpan AverageTimeToDeliver { get; protected set; }

        [DataMember(Name = "AverageTimeToProcess")]
        public TimeSpan AverageTimeToProcess { get; protected set; }

        [DataMember(Name = "MonitoringPeriod")]
        public TimeSpan MonitoringPeriod { get; protected set; }

        public HermesEndpointPerformanceMetricReceived(EndpointPerformanceEventArgs e)
        {
            Endpoint = e.Endpoint;
            TotalMessagesProcessed = e.TotalMessagesProcessed;
            TotalErrorMessages = e.TotalErrorMessages;
            AverageTimeToDeliver = e.AverageTimeToDeliver;
            AverageTimeToProcess = e.AverageTimeToProcess;
            MonitoringPeriod = e.MonitoringPeriod;
        }
    }
}
