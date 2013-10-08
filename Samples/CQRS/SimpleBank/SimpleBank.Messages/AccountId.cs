using System;
using System.Runtime.Serialization;

using Hermes;
using Hermes.Domain;

namespace SimpleBank.Messages
{
    [DataContract]
    public class AccountId : Identity<Guid>
    {
        protected AccountId()
        {
        }

        public AccountId(Guid id)
            : base(id)
        {

        }

        public static AccountId New()
        {
            return new AccountId(SequentialGuid.New());
        }
    }
}