using Hermes.Domain;

namespace SimpleBank.Messages.Events
{
    public class AccountDebited : DomainEvent
    {
        public decimal Amount { get; protected set; }
        public decimal CurrentBalance { get; protected set; }

        public AccountDebited(decimal amount, decimal currentBalance)
        {
            Amount = amount;
            CurrentBalance = currentBalance;
        }
    }
}