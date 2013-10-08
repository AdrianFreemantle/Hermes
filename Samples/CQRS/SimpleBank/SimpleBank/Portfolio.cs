using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Domain;

using SimpleBank.Messages;
using SimpleBank.Messages.Events;

namespace SimpleBank
{
    public partial class Portfolio : Aggregate
    {
        private static readonly Money MinimumPortfolioBalance = Money.Amount(100);

        protected Portfolio(PortfolioId id) 
            : base(id)
        {
            accounts = new HashSet<Account>();
        }

        public static Portfolio Open(PortfolioId id, AccountType accountType, Money initialDeposit)
        {
            if (initialDeposit < MinimumPortfolioBalance)
            {
                throw new DomainRuleException(String.Format("The intial deposit of {0} is lower than the require a minimum of {1}", initialDeposit, MinimumPortfolioBalance));
            }

            var portfolio = new Portfolio(id);
            portfolio.RaiseEvent(new PortfolioOpened());
            portfolio.OpenAccount(accountType);
            portfolio.CreditAccount(accountType, initialDeposit);

            return portfolio;
        }

        public void ClosePortfolio()
        {
            if (isOpen)
            {
                RaiseEvent(new PortfolioClosed());
            }
        }

        public void OpenAccount(AccountType accountType)
        {
            GaurdPortfolioState();
            RaiseEvent(new AccountOpened(AccountId.New(), accountType));            
        }

        public void DebitAccount(AccountType accountType, Money debitAmout)
        {
            GaurdPortfolioState();
            var currentBalance = GetPortfolioBalance();

            if ((currentBalance - debitAmout) < MinimumPortfolioBalance)
            {
                RaiseEvent(new PortfolioHadInsufficientFundsForDebit(debitAmout, currentBalance, MinimumPortfolioBalance));
                return;
            }

            var account = Get<Account>(a => a.AccountType == accountType);
            account.Debit(debitAmout);
        }

        public void CreditAccount(AccountType accountType, Money creditAmount)
        {
            GaurdPortfolioState();
            var account = Get<Account>(a => a.AccountType == accountType);
            account.Credit(creditAmount);
        }

        public Money GetAccountBalance(AccountType accountType)
        {
            var account = Get<Account>(a => a.AccountType == accountType);
            return account.Balance;
        }

        public Money GetPortfolioBalance()
        {
            return accounts.Aggregate(Money.Zero, (current, account) => current + account.Balance);
        }

        private void GaurdPortfolioState()
        {
            if (!isOpen)
            {
                throw new DomainRuleException("Account is closed");
            }
        }
    }
}