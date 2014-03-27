using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntegrationTest.Client.Persistence
{
    public class Record
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual Guid Id { get; set; }

        public virtual int RecordNumber { get; set; }
        public virtual ICollection<RecordLog> RecordLogs { get; set; }
    }
}