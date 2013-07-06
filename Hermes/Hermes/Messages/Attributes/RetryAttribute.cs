using System;

namespace Hermes.Messages.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RetryAttribute : Attribute
    {
        public int RetryCount { get; set; }
        public int RetryMilliseconds { get; set; }
    }
}
