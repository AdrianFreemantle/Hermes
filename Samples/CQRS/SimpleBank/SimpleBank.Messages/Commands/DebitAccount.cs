using Hermes;

namespace SimpleBank.Messages.Commands
{
    public class DebitAccount : Command
    {
        public PortfolioId PortfolioId { get; set; }
        public AccountType AccountType { get; set; }
        public decimal Amount { get; set; }
    }
}