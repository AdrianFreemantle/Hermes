using System;
using System.Runtime.Serialization;

namespace ProcessManagement.Contracts.Messages
{
    [DataContract]
    public class ReservationResult : IMessage
    {
        [DataMember(Name = "Result")]
        public ReservationResultCode Result { get; set; }

        [DataMember(Name = "ReservationId")]
        public Guid ReservationId { get; set; }
    }
}