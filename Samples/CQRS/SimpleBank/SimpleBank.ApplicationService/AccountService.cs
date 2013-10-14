using System;

using Hermes.Domain;
using Hermes.EntityFramework;
using Hermes.Logging;
using Hermes.Messaging;

using SimpleBank.Messages;
using SimpleBank.Messages.Commands;

namespace SimpleBank.ApplicationService
{
    public class AccountService 
        : IHandleMessage<ClosePortfolio>
        , IHandleMessage<OpenPortfolio>
        , IHandleMessage<OpenAccount>
        , IHandleMessage<DebitAccount>
        , IHandleMessage<CreditAccount>
    {
        private readonly IAggregateRepository aggregateRepository;
        private readonly IMessageBus bus;
        private readonly ILog Logger = LogFactory.BuildLogger(typeof (AccountService));

        public AccountService(IAggregateRepository aggregateRepository, IMessageBus bus)
        {
            this.aggregateRepository = aggregateRepository;
            this.bus = bus;
        }

        public void Handle(OpenPortfolio message)
        {
            try
            {
                var portfolio = Portfolio.Open(message.PortfolioId, message.AccountType, Money.Amount(message.InitialDeposit));
                aggregateRepository.Add(portfolio);
                bus.Return(ReturnCodes.Success);
                Logger.Info("Completed processing of message {0} on {1}", bus.CurrentMessageContext.MessageId, message.PortfolioId);
            }
            catch (DomainRuleException ex)
            {
                HandleException(message.PortfolioId, ex);
            }
        }

        private void Update(PortfolioId id, Action<Portfolio> action)
        {
            try
            {
                var portfolio = aggregateRepository.Get<Portfolio>(id);
                action(portfolio);
                aggregateRepository.Update(portfolio);
                bus.Return(ReturnCodes.Success);
                Logger.Info("Completed processing of message {0} on {1}", bus.CurrentMessageContext.MessageId, id);
            }
            catch (DomainRuleException ex)
            {
                HandleException(id, ex);
            }
        }

        private void HandleException(PortfolioId id, DomainRuleException ex)
        {
            bus.Return(ReturnCodes.DomainRuleError, ex.Message);
            Logger.Error("Domain rule violation while processing message {0} on {1} : {2}", bus.CurrentMessageContext.MessageId, id, ex.Message);
        }

        public void Handle(ClosePortfolio message)
        {
            Action<Portfolio> action = (portfolio) => portfolio.ClosePortfolio();
            Update(message.PortfolioId, action);
        }        

        public void Handle(OpenAccount message)
        {
            Action<Portfolio> action = (portfolio) => portfolio.OpenAccount(message.AccountType);
            Update(message.PortfolioId, action);
        }

        public void Handle(DebitAccount message)
        {
            Action<Portfolio> action = (portfolio) => portfolio.DebitAccount(message.AccountType, Money.Amount(message.Amount));
            Update(message.PortfolioId, action);
        }

        public void Handle(CreditAccount message)
        {
            Action<Portfolio> action = (p) => p.CreditAccount(message.AccountType, Money.Amount(message.Amount));
            Update(message.PortfolioId, action);
        }
    }
}
