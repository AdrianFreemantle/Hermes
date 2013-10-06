namespace Hermes.Messaging
{
    public class Headers
    {
        public const string RetryCount = "Hermes.Retry.Count";
        public const string FailureDetails = "Hermes.Failed.Exception";
        public const string TimeoutExpire = "Hermes.Timeout.Expire";
        public const string RouteExpiredTimeoutTo = "Hermes.Timeout.RouteExpiredTimeoutTo";
        public const string OriginalReplyToAddress = "Hermes.Timeout.ReplyToAddress;";
        public const string ControlMessageHeader = "Hermes.ControlMessage";
        public const string ReturnMessageErrorCodeHeader = "Hermes.ReturnMessageErrorCode";
    }
}