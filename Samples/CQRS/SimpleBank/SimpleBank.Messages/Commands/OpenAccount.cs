using Hermes;

namespace SimpleBank.Messages.Commands
{
    public class OpenAccount : Command
    {
        public PortfolioId PortfolioId { get; set; }
        public AccountType AccountType { get; set; }
    }
}