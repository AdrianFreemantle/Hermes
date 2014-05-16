using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using Hermes.Persistence;
using ServiceStack.OrmLite;

namespace Hermes.OrmLite
{
    [Obsolete("No longer supported", true)]
    public class OrmLiteUnitOfWork : IUnitOfWork
    {
        private readonly List<Action<IDbConnection>> actions = new List<Action<IDbConnection>>();
        private readonly IDbConnectionFactory dbConnectionFactory;
        
        public OrmLiteUnitOfWork(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public void Commit()
        {
            if (actions.Any())
            {
                using(var scope = TransactionScopeUtils.Begin(TransactionScopeOption.Required))
                using (var connection = dbConnectionFactory.OpenDbConnection())
                {
                    foreach (var action in actions)
                    {
                        action.Invoke(connection);
                    }

                    scope.Complete();
                }
            }
        }

        public void Rollback()
        {
            actions.Clear();
        }

        public void Dispose()
        {
            //no-op
        }

        public void AddAction(Action<IDbConnection> databaseAction)
        {
            actions.Add(databaseAction);
        }
    }
}