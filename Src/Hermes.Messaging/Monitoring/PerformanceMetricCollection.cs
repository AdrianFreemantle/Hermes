using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Logging;
using Hermes.Messaging.Pipeline;

namespace Hermes.Messaging.Monitoring
{
    public class PerformanceMetricCollection
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (PerformanceMetricCollection));
        private readonly List<MessagePerformanceMetric> performanceMetrics = new List<MessagePerformanceMetric>();
        private readonly object syncLock = new object();

        public PerformanceMetric GetEndpointPerformance()
        {
            lock (syncLock)
            {
                PerformanceMetric performanceMetric = GetPerformanceMetric();
                performanceMetrics.Clear();
                return performanceMetric;
            }
        }

        private PerformanceMetric GetPerformanceMetric()
        {
            TimeSpan attd = GetAverageTimeToDelivery();
            TimeSpan attp = GetAverageTimeToProcess();
            int completedCount = CompletedCount();
            int errorCount = ErrorCount();

            if (attd < TimeSpan.Zero) //due to doubles being floating points we may end up with less than zero
                attd = TimeSpan.Zero;

            if (attp < TimeSpan.Zero)
                attp = TimeSpan.Zero;

            return new PerformanceMetric(attp, attd, completedCount, errorCount);
        }

        public void Add(DateTime receivedTime, IncomingMessageContext incomingMessage)
        {
            try
            {
                var metric = new MessagePerformanceMetric(receivedTime, incomingMessage);

                lock (syncLock)
                {
                    performanceMetrics.Add(metric);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error while adding performance metric. {0}", ex.GetFullExceptionMessage());
            }
        }

        private TimeSpan GetAverageTimeToDelivery()
        {
            var metrics = CompletedMetrics();

            if (metrics.Any())
            {
                return TimeSpan.FromTicks((long)metrics.Average(metric => metric.TimeToDeliver.Ticks));
            }

            return TimeSpan.Zero;
        }

        private TimeSpan GetAverageTimeToProcess()
        {
            var metrics = CompletedMetrics();

            if (metrics.Any())
            {
                return TimeSpan.FromTicks((long)metrics.Average(metric => metric.TimeToProcess.Ticks));
            }

            return TimeSpan.Zero;
        }

        private ICollection<MessagePerformanceMetric> CompletedMetrics()
        {
            return performanceMetrics.Where(metric => !metric.Error).ToArray();
        }

        private int ErrorCount()
        {
            return performanceMetrics.Count(metric => metric.Error);
        }

        private int CompletedCount()
        {
            return performanceMetrics.Count(metric => !metric.Error);
        }
    }
}