using System;

namespace Hermes.Messaging.Monitoring
{
    public struct PerformanceMetric
    {
        public TimeSpan AverageTimeToProcess { get; private set; }
        public TimeSpan AverageTimeToDeliver { get; private set; }
        public int TotalMessagesProcessed { get; private set; }
        public int TotalErrorMessages { get; private set; }

        public PerformanceMetric(TimeSpan averageTimeToProcess, TimeSpan averageTimeToDeliver, int totalMessagesProcessed, int totalErrorMessages)
            :this()
        {
            AverageTimeToDeliver = averageTimeToDeliver;
            AverageTimeToProcess = averageTimeToProcess;
            TotalMessagesProcessed = totalMessagesProcessed;
            TotalErrorMessages = totalErrorMessages;
        }
    }
}