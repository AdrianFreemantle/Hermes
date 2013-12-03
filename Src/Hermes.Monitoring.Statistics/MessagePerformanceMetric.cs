using System;
using System.Collections.Generic;

using Hermes.Messaging;
using Hermes.Serialization.Json;

namespace Hermes.Monitoring.Statistics
{
    public struct MessagePerformanceMetric
    {
        public TimeSpan TimeToProcess { get; private set; }
        public TimeSpan TimeToDeliver { get; private set; }
        public Address Endpoint { get; private set; }

        public MessagePerformanceMetric(Dictionary<string, string> headers)
            :this()
        {
            Mandate.ParameterNotNull(headers, "headers");
            DateTime completedTime = headers[HeaderKeys.CompletedTime].ToUtcDateTime();
            DateTime sentTime = headers[HeaderKeys.SentTime].ToUtcDateTime();
            DateTime receivedTime = headers[HeaderKeys.ReceivedTime].ToUtcDateTime();
            
            Endpoint = Address.Parse(headers[HeaderKeys.ProcessingEndpoint]);
            TimeToProcess = completedTime - receivedTime;
            TimeToDeliver = receivedTime - sentTime;
        }
    }
}