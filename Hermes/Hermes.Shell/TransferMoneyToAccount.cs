using Hermes.Messages;
using Hermes.Messages.Attributes;

namespace Hermes.Shell
{
    [Retry(RetryCount = 3, RetryMilliseconds = 300)]
    public class TransferMoneyToAccount : IMessage
    {
        public string Hello { get; set; }
    }
}