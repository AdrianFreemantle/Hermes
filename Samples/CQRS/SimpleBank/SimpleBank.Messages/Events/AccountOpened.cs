using Hermes.Domain;

namespace SimpleBank.Messages.Events
{
    public class AccountOpened : DomainEvent
    {
        public AccountType AccountType { get; protected set; }
        public AccountId AccountId { get; protected set; }

        public AccountOpened(AccountId accountId, AccountType accountType)
        {
            AccountType = accountType;
            AccountId = accountId;
        }
    }
}