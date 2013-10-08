using Hermes;

namespace SimpleBank.Messages.Commands
{
    public class ClosePortfolio : Command
    {
        public PortfolioId PortfolioId { get; set; }
    }
}