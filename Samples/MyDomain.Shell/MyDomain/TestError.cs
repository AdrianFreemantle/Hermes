using System;

namespace MyDomain
{
    public static class TestError
    {
        private static readonly Random Random = new Random();

        public static void Throw()
        {
            if (Random.Next() % 12 == 0)
            {
                throw new Exception("=== Test Error ===");
            }
        }
    }
}
