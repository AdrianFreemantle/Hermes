namespace Hermes.Messaging
{
    public class HeaderKeys
    {
        public const string MessageType = "Hermes.MessageType";
        public const string SentTime = "Hermes.SentTime";
        public const string ReceivedTime = "Hermes.ReceivedTime";
        public const string CompletedTime = "Hermes.CompletedTime";
        public const string FirstLevelRetryCount = "Hermes.Retry.Count";
        public const string SecondLevelRetryCount = "Hermes.SecondLevelRetry.Count";
        public const string FailureDetails = "Hermes.Failed.Exception";
        public const string TimeoutExpire = "Hermes.Timeout.Expire";
        public const string RouteExpiredTimeoutTo = "Hermes.Timeout.RouteExpiredTimeoutTo";
        public const string OriginalReplyToAddress = "Hermes.Timeout.ReplyToAddress;";
        public const string ControlMessageHeader = "Hermes.ControlMessage";
        public const string ReturnErrorCode = "Hermes.ReturnErrorCode";
        public const string ProcessingEndpoint = "Hermes.ProcessingEndpoint";
        public const string UserName = "Hermes.UserName";
        public const string SentMessageCounter = "Hermes.SentMessageCounter";
    }
}