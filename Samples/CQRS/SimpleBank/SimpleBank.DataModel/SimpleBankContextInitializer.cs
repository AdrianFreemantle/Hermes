using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

using Hermes.EntityFramework;

using SimpleBank.DataModel.ReadModel;
using SimpleBank.Messages;

namespace SimpleBank.DataModel
{
    public class SimpleBankContextInitializer : IDatabaseInitializer<SimpleBankContext>
    {
        public void InitializeDatabase(SimpleBankContext context)
        {
            InitDatabase(context);
        }

        private void InitDatabase(SimpleBankContext context)
        {
            if (!context.Database.Exists())
            {
                context.Database.Create();
            }

            context.RegisterLookupTable<AccountTypeLookup, AccountType>();
            context.SaveLookupTableChanges(typeof (AccountTypeLookup));
        }
    }

    public class PortfolioRecordConfiguration : EntityTypeConfiguration<PortfolioRecord>
    {
        public PortfolioRecordConfiguration()
        {
            HasKey(p => p.Id);
            Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            
            HasMany(p => p.Accounts).WithRequired(a => a.Portfolio).WillCascadeOnDelete(false);
        }
    }

    public class AccountRecordConfiguration : EntityTypeConfiguration<AccountRecord>
    {
        public AccountRecordConfiguration()
        {
            HasKey(p => p.Id);
            Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            HasMany(p => p.Transactions).WithRequired(p => p.AccountRecord).WillCascadeOnDelete(false);
        }
    }
}
