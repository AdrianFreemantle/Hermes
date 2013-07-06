using System.Transactions;

namespace Hermes.Transports.SqlServer
{
    public class TransactionScopeUtils
    {
        public static TransactionScope Begin()
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            };

            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }
    }
}