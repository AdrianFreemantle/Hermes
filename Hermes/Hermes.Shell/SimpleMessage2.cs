using Hermes.Messages;
using Hermes.Messages.Attributes;

namespace Hermes.Shell
{
    [Retry(RetryCount = 3, RetryMilliseconds = 300)]
    public class SimpleMessage2 : IMessage
    {
        public string Hello { get; set; }
    }
}