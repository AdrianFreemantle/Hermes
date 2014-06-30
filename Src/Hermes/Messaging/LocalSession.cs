using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public class LocalSession
    {
        public int Id { get; set; }
        public Guid MessageId { get; set; }
        public object Message { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}