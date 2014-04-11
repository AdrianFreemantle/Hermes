using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.EntityFramework.KeyValueStore
{
    [Table("KeyValueStore")]
    public class KeyValueEntity : IPersistenceAudit 
    {
        [Key, StringLength(40), DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public string Id { get; set; }

        [Required, StringLength(256), Index(IsUnique = true)] 
        public string Key { get; set; }
       
        [Required, StringLength(1024)] 
        public string ValueType { get; set; }
       
        [Required]
        public byte[] Value { get; set; }
       
        [Timestamp] 
        public byte[] TimeStamp { get; set; }

        [Required, StringLength(128)] 
        public string ModifiedBy { get; set; }

        [Required, StringLength(128)] 
        public string CreatedBy { get; set; }
       
        public DateTime ModifiedTimestamp { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }
}