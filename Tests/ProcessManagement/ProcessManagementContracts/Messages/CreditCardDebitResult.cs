using System.Runtime.Serialization;

namespace ProcessManagement.Contracts.Messages
{
    [DataContract]
    public class CreditCardDebitResult : IMessage
    {
        [DataMember(Name = "PaymentCompleted")]
        public bool PaymentCompleted { get; set; }
    }
}
