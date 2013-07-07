using Hermes.Messages;
using Hermes.Messages.Attributes;

namespace Hermes.Shell
{
    [Retry(RetryCount = 3, RetryMilliseconds = 200)]
    public class SimpleMessage : IMessage
    {
        public string Hello { get; set; }
    }
}