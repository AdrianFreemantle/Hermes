using System;

namespace Hermes.Messaging
{
    [Serializable]
    public class CompletionResult
    {
        public int ErrorCode { get; set; }
        public object Message { get; set; }
        public object State { get; set; }
    }
}
