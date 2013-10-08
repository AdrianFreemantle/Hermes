using System;

using Hermes;
using Hermes.Domain;

using SimpleBank.Messages;
using SimpleBank.Messages.Events;

namespace SimpleBank
{
    public partial class Account : Entity
    {
        protected Account(Portfolio portfolio, AccountId identity)
            : base(portfolio, identity)
        {
        }

        internal static Account Open(Portfolio portfolio, AccountId identity, AccountType accountType)
        {
            return new Account(portfolio, identity) 
            {
                AccountType = accountType
            };
        }

        public void Debit(Money amount)
        {
            if (Balance - amount < 0)
            {
                RaiseEvent(new AccountHadInsufficientFundsForDebit(amount, Balance));
                return;
            }

            RaiseEvent(new AccountDebited(amount, Balance));
        }

        public void Credit(Money amount)
        {
            RaiseEvent(new AccountCredited(amount, Balance));
        }
    }
}