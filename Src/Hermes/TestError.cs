using System;

using Hermes.Logging;

namespace Hermes
{
    public class HermesTestingException : Exception
    {
        public HermesTestingException()
            : base("=== Test Error ===")
        {
        }
    }

    public static class TestError
    {
        private static readonly Random Random = new Random();
        private static ILog logger = LogFactory.BuildLogger(typeof (TestError));

        public static int PercentageFailure { get; set; }

        public static void Throw()
        {
            var rand = Random.Next(1, 100);

            if (rand <= PercentageFailure)
            {
                logger.Verbose("====== TEST ERROR THROWN =======");
                throw new HermesTestingException();
            }
        }
    }
}
