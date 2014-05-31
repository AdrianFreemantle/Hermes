using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalBus.Persistence
{
    public class RecordLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        public virtual Guid RecordId { get; set; }
        public virtual Record Record { get; set; }
    }
}