using Hermes.Domain;

namespace SimpleBank.Messages.Events
{
    [EventDoesNotMutateState]
    public class PortfolioHadInsufficientFundsForDebit : DomainEvent
    {
        public decimal DebitAmount { get; protected set; }
        public decimal CurrentBalance { get; protected set; }
        public decimal MinimumBalance { get; protected set; }

        public PortfolioHadInsufficientFundsForDebit(decimal debitAmount, decimal currentBalance, decimal minimumBalance)
        {
            DebitAmount = debitAmount;
            CurrentBalance = currentBalance;
            MinimumBalance = minimumBalance;
        }
    }
}