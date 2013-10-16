namespace Hermes.Messaging
{
    public class CompletionResult
    {
        public int ErrorCode { get; set; }
        public object[] Messages { get; set; }
        public object State { get; set; }
    }
}
