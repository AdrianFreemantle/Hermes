using System;

using Hermes.EntityFramework;
using Hermes.Messaging;

using SimpleBank.DataModel.ReadModel;
using SimpleBank.Messages.Events;

namespace SimpleBank.ApplicationService.Projections
{
    public class PortfolioProjections 
        : IHandleMessage<AccountCredited>
        , IHandleMessage<AccountDebited>
        , IHandleMessage<AccountHadInsufficientFundsForDebit>
        , IHandleMessage<PortfolioClosed>
        , IHandleMessage<PortfolioOpened>
        , IHandleMessage<PortfolioHadInsufficientFundsForDebit>
        , IHandleMessage<AccountOpened>
    {
        private readonly IUnitOfWork unitOfWork;

        public PortfolioProjections(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void Handle(AccountCredited message)
        {
            var portfolios = unitOfWork.GetRepository<PortfolioRecord>();
            var accounts = unitOfWork.GetRepository<AccountRecord>();
            var transactions = unitOfWork.GetRepository<TransactionRecord>();

            var accountRecord = accounts.Get(message.EntityId.GetId());
            var portfolio = portfolios.Get(message.AggregateId.GetId());

            accountRecord.CurrentBalance = message.CurrentBalance;

            if (message.Amount > 100)
            {
                portfolio.CustomerReliabilityScore++;
            }

            portfolio.TotalBalance += message.Amount;

            transactions.Add(new TransactionRecord
            {
                AccountRecordId = (Guid)message.EntityId.GetId(),
                Amount = message.Amount
            });
        }

        public void Handle(AccountDebited message)
        {
            var portfolios = unitOfWork.GetRepository<PortfolioRecord>();
            var accounts = unitOfWork.GetRepository<AccountRecord>();
            var transactions = unitOfWork.GetRepository<TransactionRecord>();

            var accountRecord = accounts.Get(message.EntityId.GetId());
            var portfolio = portfolios.Get(message.AggregateId.GetId());

            accountRecord.CurrentBalance = message.CurrentBalance;
            portfolio.TotalBalance += message.Amount;

            transactions.Add(new TransactionRecord
            {
                AccountRecordId = message.EntityId.GetId(),
                Amount = -message.Amount
            });
        }

        public void Handle(PortfolioClosed message)
        {
            var portfolios = unitOfWork.GetRepository<PortfolioRecord>();
            var portfolio = portfolios.Get(message.AggregateId.GetId());

            portfolio.IsOpen = false;
        }

        public void Handle(PortfolioOpened message)
        {
            var portfolios = unitOfWork.GetRepository<PortfolioRecord>();

            portfolios.Add(new PortfolioRecord
            {
                Id = message.AggregateId.GetId(), 
                IsOpen = true
            });
        }

        public void Handle(AccountHadInsufficientFundsForDebit message)
        {
            var portfolios = unitOfWork.GetRepository<PortfolioRecord>();

            var portfolio = portfolios.Get(message.AggregateId.GetId());
            portfolio.CustomerReliabilityScore -= 1;
        }

        public void Handle(PortfolioHadInsufficientFundsForDebit message)
        {
            var portfolios = unitOfWork.GetRepository<PortfolioRecord>();

            var portfolio = portfolios.Get(message.AggregateId.GetId());
            portfolio.CustomerReliabilityScore--;
        }

        public void Handle(AccountOpened message)
        {
            var accounts = unitOfWork.GetRepository<AccountRecord>();

            accounts.Add(new AccountRecord
            {
                AccountType = new AccountTypeLookup(message.AccountType),
                Id = message.AccountId.GetId(),
                PortfolioId = message.AggregateId.GetId()
            });
        }
    }
}
