using System;
using System.ComponentModel.DataAnnotations;
using Hermes.Messaging;

namespace ProcessManagement.ProcessManager
{
    public class SeatReservationProcessManagerState : IContainProcessManagerData
    {
        [Required]
        public virtual Guid Id { get; set; }

        [Required]
        public virtual Guid OriginalMessageId { get; set; }

        [Required]
        public virtual string Originator { get; set; }

        [Required]
        public virtual int Version { get; set; }

        [Required]
        public virtual Guid TimeoutId { get; set; }

        [Timestamp]
        public virtual byte[] TimeStamp { get; set; }
    }
}