using Hermes.Domain;

namespace SimpleBank.Messages.Events
{
    [EventDoesNotMutateState]
    public class AccountHadInsufficientFundsForDebit : DomainEvent
    {
        public decimal DebitAmount { get; protected set; }
        public decimal CurrentBalance { get; protected set; }

        public AccountHadInsufficientFundsForDebit(decimal debitAmount, decimal currentBalance)
        {
            DebitAmount = debitAmount;
            CurrentBalance = currentBalance;
        }
    }
}