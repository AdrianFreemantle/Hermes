using System;

using Hermes.Messaging;

namespace Hermes.Monitoring.Statistics
{
    public struct EndpointPerformanceMetric
    {
        public TimeSpan AverageTimeToProcess { get; private set; }
        public TimeSpan AverageTimeToDeliver { get; private set; }
        public int TotalMessagesProcessed { get; private set; }
        public Address Endpoint { get; private set; }

        public EndpointPerformanceMetric(Address endpoint, TimeSpan averageTimeToProcess, TimeSpan averageTimeToDeliver, int totalMessagesProcessed)
            :this()
        {
            Endpoint = endpoint;
            AverageTimeToDeliver = averageTimeToDeliver;
            AverageTimeToProcess = averageTimeToProcess;
            TotalMessagesProcessed = totalMessagesProcessed;
        }
    }
}