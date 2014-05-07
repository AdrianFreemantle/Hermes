using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hermes.Logging;
using Hermes.Monitoring.Statistics;
using Hermes.Serialization.Json;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            LogFactory.BuildLogger = t => new ConsoleWindowLogger(t);
            var counter = new SqlTransportPerfomanceMonitor(new JsonObjectSerializer());
            counter.OnPerformancePeriodCompleted += counter_OnPerformancePeriodCompleted;

            while (true)
            {
                System.Threading.Thread.Sleep(10);
            }
        }

        static void counter_OnPerformancePeriodCompleted(object sender, PerformanceMetricEventArgs e)
        {
            if (e.PerformanceMetric.Any())
            {
                PrintMetrics(e);
            }
            else
            {
                Console.WriteLine("No messages processed.");
            }
           
        }

        private static void PrintMetrics(PerformanceMetricEventArgs e)
        {
            Console.WriteLine("================================================================================");

            Console.WriteLine("[Total MSG: {0}] [ATTD: {1} ] [ATTP: {2}]", 
                e.PerformanceMetric.Count, 
                e.PerformanceMetric.AverageTimeToDelivery, 
                e.PerformanceMetric.AverageTimeToProcess);

            foreach (var metric in e.PerformanceMetric.GetEndpointPerformance())
            {
                Console.WriteLine("[Endpoint: {0}] [MSG: {1}] [MSG/S: {2}] [ATTD: {3} ] [ATTP: {4}]",
                                  metric.Endpoint, 
                                  metric.TotalMessagesProcessed,
                                  (metric.TotalMessagesProcessed / e.MonitorPeriod.Seconds),
                                  metric.AverageTimeToDeliver.Milliseconds,
                                  metric.AverageTimeToProcess.Milliseconds);
            }
            
        }
    }
}
