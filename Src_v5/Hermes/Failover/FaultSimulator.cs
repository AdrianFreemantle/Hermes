using System;

namespace Hermes.Failover
{
    public static class FaultSimulator
    {
        private static readonly object SyncLock = new object();

        private static Random rand = new Random(DateTime.Now.GetHashCode());
        private static decimal percentageChance;
        private const int DecimalPointShift = 1000000;
        private const int OneHundredPercent = 100 * DecimalPointShift;
        private static int tripCount;

        public static void SetPercetageChanceOfErrorBeingThrown(decimal percentage)
        {
            percentageChance = percentage * DecimalPointShift;
        }

        public static void Trigger()
        {
            if (percentageChance <= 0)
                return;

            lock (SyncLock)
            {
                if (tripCount > 10)
                {
                    //there appears to be a problem with Random. It starts returning 1 for each call after a certain amount of iterations
                    rand = new Random(DateTime.Now.GetHashCode());
                }

                int triggerValue = rand.Next(1, OneHundredPercent);

                if (triggerValue < percentageChance)
                {
                    tripCount++;
                    throw new HermesTestingException();
                }

                tripCount = 0;
            }
        }
    }
}