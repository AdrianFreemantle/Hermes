using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Hermes.EntityFramework;
using Hermes.Persistence;

namespace IntegrationTests.PersistenceModel
{
    public class IntegrationTestContext : FrameworkContext 
    {
        public IDbSet<Record> Records { get; set; }
        public IDbSet<RecordLog> RecordLogs { get; set; }

        public IntegrationTestContext()
        {
        }

        public IntegrationTestContext(string connectionStringName)
            :base(connectionStringName)
        {
            
        }
    }

    public class Record : ITimestampPersistenceAudit 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual Guid Id { get; set; }
        public virtual int RecordNumber { get; set; }
        public virtual ICollection<RecordLog> RecordLogs { get; set; }
        public virtual DateTime ModifiedTimestamp { get; set; }
        public virtual DateTime CreatedTimestamp { get; set; }
    }

    public class RecordLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        public virtual Guid RecordId { get; set; }
        public virtual Record Record { get; set; }
    }
}
