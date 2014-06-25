using System;
using System.Runtime.Serialization;

namespace ProcessManagement.Contracts.Events
{
    public interface IReservationCancelled : IEvent
    {
        Guid ReservationId { get; }
    }

    [DataContract]
    public class ReservationCancelled : IReservationCancelled
    {
        [DataMember(Name = "ReservationId")]
        public Guid ReservationId { get; protected set; }

        public ReservationCancelled(Guid reservationId)
        {
            ReservationId = reservationId;
        }
    }
}