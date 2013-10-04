using System;

namespace Hermes.EntityFramework.SagaPersistence
{
    public class ProcessManagerEntity
    {
        public Guid Id { get; set; }
        public Byte[] TimeStamp { get; set; }
        public byte[] State { get; set; }
    }

    public class DeduplicationEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedTimestamp { get; set; }

        public DeduplicationEntity()
        {
            CreatedTimestamp = DateTime.Now;
        }
    }
}