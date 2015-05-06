using System;
using System.Data.Entity;
using System.Data.SqlClient;
using Hermes.Messaging.Transports.SqlTransport;

namespace Hermes.EntityFramework.Queues
{
    public class QueueFactory 
    {
        private readonly EntityFrameworkUnitOfWork unitOfWork;

        public QueueFactory(EntityFrameworkUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void CreateQueueIfNecessary(string queueName)
        {
            Mandate.ParameterNotNullOrEmpty(queueName, "queueName");

            Database database = unitOfWork.GetDatabase();

            using (var connection = new SqlConnection(database.Connection.ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(String.Format(QueueSqlCommands.CreateQueue, queueName.ToUriSafeString()), connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}