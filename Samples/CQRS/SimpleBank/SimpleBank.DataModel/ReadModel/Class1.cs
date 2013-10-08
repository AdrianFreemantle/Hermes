using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Hermes.EntityFramework;
using SimpleBank.Messages;

namespace SimpleBank.DataModel.ReadModel
{
    [Table("Portfolio")]
    public class PortfolioRecord : BaseEntity
    {
        public string Id { get; set; }
        public bool IsOpen { get; set; }
        public decimal TotalBalance { get; set; }
        public int CustomerReliabilityScore { get; set; }
        public ICollection<AccountRecord> Accounts { get; set; }

        public PortfolioRecord()
        {
            Accounts = new List<AccountRecord>();
        }
    }

    [Table("Account")]
    public class AccountRecord : BaseEntity
    {
        public Guid Id { get; set; }

        public string PortfolioId { get; set; }
        public PortfolioRecord Portfolio { get; set; }

        public decimal CurrentBalance { get; set; }
        public ICollection<TransactionRecord> Transactions { get; set; }

        public int AccountTypeId { get; set; }
        public AccountTypeLookup AccountType { get; set; }

        public AccountRecord()
        {
            Transactions = new List<TransactionRecord>();
        }
    }

    [Table("Transaction")]
    public class TransactionRecord
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }

        public Guid AccountRecordId { get; set; }
        public AccountRecord AccountRecord { get; set; }
    }

    [Table("AccountType")]
    public class AccountTypeLookup : EnumWrapper<AccountType>
    {
        protected AccountTypeLookup()
        {
        }

        public AccountTypeLookup(AccountType value) : base(value)
        {
        }
    }


}
