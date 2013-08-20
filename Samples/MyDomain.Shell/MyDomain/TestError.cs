using System;

namespace MyDomain
{
    public static class TestError
    {
        private static readonly Random Random = new Random();

        public static void Throw()
        {
            var rand = Random.Next(0, 100);

            if (rand == 50)
            {
                throw new Exception("=== Test Error ===");
            }
        }
    }
}
