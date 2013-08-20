namespace Hermes.Core
{
    public class MessageHeaders
    {
        public const string Count = "Hermes.Retry.Count";
        public const string Failed = "Hermes.Failed.Exception";
        public const string Expire = "Hermes.Timeout.Expire";
        public const string RouteExpiredTimeoutTo = "Hermes.Timeout.RouteExpiredTimeoutTo";
        public const string ClearTimeouts = "Hermes.ClearTimeouts";
        public const string IsDeferredMessage = "Hermes.IsDeferredMessage";
    }
}