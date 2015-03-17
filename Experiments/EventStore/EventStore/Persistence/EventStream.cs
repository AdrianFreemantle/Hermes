using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hermes.Persistence;

namespace EventStore.Persistence
{
    [Table("EventStream")]
    public class EventStream : IPersistenceAudit
    {
        [Key]
        public virtual long CheckpointToken { get; set; }

        [Required, Index(IsUnique = true)]
        public virtual Guid ResourceId { get; set; } 

        [Required, StringLength(40)]
        [Index("IX_Commits_CommitSequence", 1, IsUnique = true)]
        [Index("IX_Commits_CommitId", 1, IsUnique = true)]
        [Index("IX_Commits_Revisions", 1, IsUnique = true)]
        public virtual string BucketId { get; set; }

        [Required, StringLength(1000)]        
        public virtual string StreamId { get; set; }

        [Required, Index, StringLength(40)] 
        [Index("IX_Commits_CommitSequence", 2, IsUnique = true)]
        [Index("IX_Commits_CommitId", 2, IsUnique = true)]
        [Index("IX_Commits_Revisions", 2, IsUnique = true)]
        public virtual string StreamIdHash { get; set; }

        [Required]
        [Index("IX_Commits_CommitId", 3, IsUnique = true)]
        public virtual Guid CommitId { get; set; }

        [Required]
        [Index("IX_Commits_Revisions", 3, IsUnique = true)]
        public int StreamRevision { get; set; }

        [Required]
        [Index("IX_Commits_CommitSequence", 3, IsUnique = true)]
        public virtual int CommitSequence { get; set; }

        [Required]
        [Index("IX_Commits_Revisions", 4, IsUnique = true)]
        public int Items { get; set; }
       
        [Required]
        public virtual bool Dispatched { get; set; }      

        [Required]
        public byte[] Headers { get; set; }

        [Required]
        public byte[] Payload { get; set; }

        [Timestamp]
        public byte[] TimeStamp { get; set; }

        [StringLength(120)]
        public string ModifiedBy { get; set; }

        [StringLength(120)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime ModifiedTimestamp { get; set; }

        [Required]
        public DateTime CreatedTimestamp { get; set; }
    }
}
