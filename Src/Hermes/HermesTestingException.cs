using System;

namespace Hermes
{
    public class HermesTestingException : Exception
    {
        public HermesTestingException()
            : base("=== Test Error ===")
        {
        }
    }
}
