using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Hermes.Messaging;

namespace Hermes.Monitoring.Statistics
{
    public class PerformanceMetricCollection : ICollection<MessagePerformanceMetric>
    {
        readonly List<MessagePerformanceMetric> performanceMetrics = new List<MessagePerformanceMetric>();
        
        public int Count { get { return performanceMetrics.Count; } }
        public bool IsReadOnly { get { return false; } }

        public TimeSpan AverageTimeToDelivery
        {
            get { return GetAverageTimeToDelivery(performanceMetrics); }
        }

        public TimeSpan AverageTimeToProcess
        {
            get { return GetAverageTimeToProcess(performanceMetrics); }
        }

        public IEnumerable<EndpointPerformanceMetric> GetEndpointPerformance()
        {
            var endpointPerformanceMetrics = new List<EndpointPerformanceMetric>();

            foreach (var endpoint in GetEndpoints())
            {
                var metrics = GetMetricsMatchingEndpoint(endpoint);
                var attd = GetAverageTimeToDelivery(metrics);
                var attp = GetAverageTimeToProcess(metrics);

                endpointPerformanceMetrics.Add(new EndpointPerformanceMetric(endpoint, attp, attd, metrics.Count));
            }

            return endpointPerformanceMetrics;
        }

        private IEnumerable<Address> GetEndpoints()
        {
            return performanceMetrics.Select(m => m.Endpoint).Distinct();
        }

        private ICollection<MessagePerformanceMetric> GetMetricsMatchingEndpoint(Address endpoint)
        {
            return performanceMetrics.Where(metric => metric.Endpoint == endpoint).ToArray();
        }

        private TimeSpan GetAverageTimeToDelivery(ICollection<MessagePerformanceMetric> metrics)
        {
            if (metrics.Any())
            {
                return TimeSpan.FromTicks((long)metrics.Average(metric => metric.TimeToDeliver.Ticks));
            }

            return TimeSpan.Zero;
        }

        private TimeSpan GetAverageTimeToProcess(ICollection<MessagePerformanceMetric> metrics)
        {
            if (metrics.Any())
            {
                return TimeSpan.FromTicks((long)metrics.Average(metric => metric.TimeToProcess.Ticks));
            }

            return TimeSpan.Zero;
        }

        public IEnumerator<MessagePerformanceMetric> GetEnumerator()
        {
            return performanceMetrics.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(MessagePerformanceMetric item)
        {
            performanceMetrics.Add(item);
        }

        public void Clear()
        {
            performanceMetrics.Clear();
        }

        public bool Contains(MessagePerformanceMetric item)
        {
            return performanceMetrics.Contains(item);
        }

        public void CopyTo(MessagePerformanceMetric[] array, int arrayIndex)
        {
            performanceMetrics.CopyTo(array, arrayIndex);
        }

        public bool Remove(MessagePerformanceMetric item)
        {
            return performanceMetrics.Remove(item);
        }       
    }
}