using System;

using Hermes.Domain;

using SimpleBank.Messages;
using SimpleBank.Messages.Events;
using SimpleBank.Snapshots;

namespace SimpleBank
{
    public partial class Account
    {
        public Money Balance { get; private set; }
        public AccountType AccountType { get; private set; }

        protected void When(AccountCredited e)
        {
            Balance += Money.Amount(e.Amount);
        }

        protected void When(AccountDebited e)
        {
            Balance = Money.Amount(Balance - Money.Amount(e.Amount));
        }

        protected override IMemento GetSnapshot()
        {
            return new AccountSnapshot
            {
                Balance = Balance,
                AccountType = AccountType
                
            };
        }        

        protected override void RestoreSnapshot(IMemento memento)
        {
            var snapshot = (AccountSnapshot)memento;

            Balance = snapshot.Balance;
            AccountType = snapshot.AccountType;
        }
    }
}