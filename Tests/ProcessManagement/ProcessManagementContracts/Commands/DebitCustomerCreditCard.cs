using System;
using System.Runtime.Serialization;

namespace ProcessManagement.Contracts.Commands
{
    [DataContract]
    public class DebitCustomerCreditCard : ICommand
    {
        [DataMember(Name = "PaymentId")]
        public Guid PaymentId { get; protected set; }

        public DebitCustomerCreditCard(Guid paymentId)
        {
            PaymentId = paymentId;
        }
    }
}