using System;

namespace Hermes.EntityFramework.SagaPersistence
{
    public class ProcessManagerEntity
    {
        public Guid Id { get; set; }
        public Byte[] TimeStamp { get; set; }
        public byte[] State { get; set; }
    }
}