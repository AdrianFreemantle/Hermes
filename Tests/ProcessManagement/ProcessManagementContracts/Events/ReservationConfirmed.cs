using System;
using System.Runtime.Serialization;

namespace ProcessManagement.Contracts.Events
{
    public interface IReservationConfirmed : IEvent
    {
        Guid ReservationId { get; }
    }

    [DataContract]
    public class ReservationConfirmed : IReservationConfirmed
    {
        [DataMember(Name = "ReservationId")]
        public Guid ReservationId { get; protected set; }

        public ReservationConfirmed(Guid reservationId)
        {
            ReservationId = reservationId;
        }
    }
}