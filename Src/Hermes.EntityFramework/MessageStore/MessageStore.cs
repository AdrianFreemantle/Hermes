using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.EntityFramework.MessageStore
{
    [Table("MessageStore")]
    public class MessageStore
    {
        public virtual int Id { get; set; }
        
        [Index(IsUnique = true)]
        public virtual Guid MessageId { get; set; }

        public bool Failed { get; set; }

        [Required]
        public virtual string Headers { get; set; }

        [Required]
        public virtual byte[] Message { get; set; }
    }
}