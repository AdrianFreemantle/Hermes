using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public class LocalSession
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public object Command { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public List<object> Events { get; set; }

        public LocalSession()
        {
        }

        public void Initialize()
    }
}