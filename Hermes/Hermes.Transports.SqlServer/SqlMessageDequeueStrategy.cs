using System;
using System.Data;
using System.Data.SqlClient;

namespace Hermes.Transports.SqlServer
{
    public class SqlMessageDequeueStrategy : IMessageDequeueStrategy
    {
        private readonly string connectionString;

        private const string SqlReceive =
            @"WITH message AS (SELECT TOP(1) * FROM [{0}] WITH (UPDLOCK, READPAST, ROWLOCK) ORDER BY [RowVersion] ASC) 
            DELETE FROM message 
            OUTPUT deleted.Id, deleted.CorrelationId, deleted.ReplyToAddress, 
            deleted.Recoverable, deleted.Expires, deleted.Headers, deleted.Body;";

        public SqlMessageDequeueStrategy(string connectionString)
        {
            this.connectionString = connectionString;
        }
      
        public void Dequeue(Func<MessageEnvelope, bool> tryProcessMessage)
        {
            using (var transactionalConnection = new TransactionalSqlConnection(connectionString))
            {
                using (var command = BuildReceiveCommand(transactionalConnection))
                {
                    TryDequeue(tryProcessMessage, command, transactionalConnection);
                }
            }           
        }

        private void TryDequeue(Func<MessageEnvelope, bool> tryProcessMessage, SqlCommand command, TransactionalSqlConnection transactionalConnection)
        {
            try
            {
                var message = ExecuteCommand(command);

                if (tryProcessMessage(message))
                {
                    transactionalConnection.Commit();
                }
                else
                {
                    transactionalConnection.Rollback();
                }
            }
            catch (Exception ex)
            {
                transactionalConnection.Rollback();
                //todo handle exception
            }
        }

        MessageEnvelope ExecuteCommand(SqlCommand command)
        {
            return null;
        }

        private SqlCommand BuildReceiveCommand(TransactionalSqlConnection connection)
        {
            var command = connection.BuildCommand(SqlReceive);
            command.CommandType = CommandType.Text;


            return command;
        }
    }
}