using System;

namespace Hermes.Messaging
{
    [Serializable]
    public class CompletionResult
    {
        public int ErrorCode { get; set; }
        public object[] Messages { get; set; }
        public object State { get; set; }
    }
}
