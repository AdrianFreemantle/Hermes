using Hermes;

namespace SimpleBank.Messages.Commands
{
    public class OpenPortfolio : Command
    {
        public PortfolioId PortfolioId { get; set; }
        public AccountType AccountType { get; set; }
        public decimal InitialDeposit { get; set; }
    }
}