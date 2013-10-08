using System;
using System.Runtime.Serialization;

using Hermes.Domain;

namespace SimpleBank.Snapshots
{
    [DataContract(Name = "PortfolioMemento")]
    public class PortfolioMemento : Memento
    {
        [DataMember(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        [DataMember(Name = "Accounts")]
        public AccountSnapshot[] Accounts { get; set; }
    }
}