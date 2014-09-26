using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hermes.Persistence;

namespace Hermes.EntityFramework.KeyValueStore
{
    [Table("KeyValueStore")]
    public class KeyValueEntity : IPersistenceAudit 
    {
        [Key]
        public int Id { get; set; }

        [Required, Index(IsUnique = true), StringLength(40), DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public string Hash { get; set; }

        [Required, StringLength(256)] 
        public string Key { get; set; }
       
        [Required, StringLength(1024)] 
        public string ValueType { get; set; }
       
        [Required]
        public byte[] Value { get; set; }
       
        [Timestamp] 
        public byte[] TimeStamp { get; set; }

        [StringLength(120)] 
        public string ModifiedBy { get; set; }

        [StringLength(120)] 
        public string CreatedBy { get; set; }
       
        public DateTime ModifiedTimestamp { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }
}