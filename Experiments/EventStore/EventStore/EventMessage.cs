using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EventStore
{
    [DataContract]
    public class EventMessage
    {
        [DataMember(Name = "Headers")]
        public Dictionary<string, string> Headers  { get; set; }
     
        [DataMember(Name = "Body")]
        public object Body { get; set; }

        public EventMessage()
        {
            Headers = new Dictionary<string, string>();
        }
    }
}