using System;
using System.Runtime.Serialization;

using Hermes.Domain;

using SimpleBank.Messages;

namespace SimpleBank.Snapshots
{
    [DataContract(Name = "AccountSnapshot")]
    public class AccountSnapshot : Memento
    {
        [DataMember(Name = "Balance")]
        public Money Balance { get; set; }

        [DataMember(Name = "AccountType")]
        public AccountType AccountType { get; set; }
    }
}