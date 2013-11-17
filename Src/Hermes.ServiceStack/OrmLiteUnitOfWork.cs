using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Hermes.Persistence;

using ServiceStack.OrmLite;

namespace Hermes.ServiceStack
{
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
                using (var connection = dbConnectionFactory.OpenDbConnection())
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    foreach (var action in actions)
                    {
                        action.Invoke(connection);
                    }

                    transaction.Commit();
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