namespace Hermes.Core.Deferment
{
    public class TimeoutHeaders
    {
        public const string Expire = "Hermes.Timeout.Expire";
        public const string RouteExpiredTimeoutTo = "Hermes.Timeout.RouteExpiredTimeoutTo";
        public const string ClearTimeouts = "Hermes.ClearTimeouts";
        public const string IsDeferredMessage = "Hermes.IsDeferredMessage";
    }
}