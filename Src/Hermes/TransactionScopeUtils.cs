using System;
using System.Transactions;

namespace Hermes
{
    public class TransactionScopeUtils
    {
        public static TransactionScope Begin()
        {
            return Begin(TransactionScopeOption.Required);
        }

        public static TransactionScope Begin(TransactionScopeOption scopeOption)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            };

            return new TransactionScope(scopeOption, transactionOptions);
        }

        public static TransactionScope Begin(TransactionScopeOption scopeOption, TimeSpan timeOut)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = timeOut
            };

            return new TransactionScope(scopeOption, transactionOptions);
        }
    }
}