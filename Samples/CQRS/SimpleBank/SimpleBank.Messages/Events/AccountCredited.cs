using Hermes.Domain;

namespace SimpleBank.Messages.Events
{
    public class AccountCredited : DomainEvent
    {
        public decimal Amount { get; protected set; }
        public decimal CurrentBalance { get; protected set; }

        public AccountCredited(decimal amount, decimal currentBalance)
        {
            Amount = amount;
            CurrentBalance = currentBalance;
        }
    }


}