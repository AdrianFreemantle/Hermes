using System;

namespace Hermes.EntityFramework.KeyValueStore
{
    public class KeyValueEntity : IPersistenceAudit 
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string ValueType { get; set; }
        public Byte[] Value { get; set; }
        public Byte[] TimeStamp { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedTimestamp { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }
}