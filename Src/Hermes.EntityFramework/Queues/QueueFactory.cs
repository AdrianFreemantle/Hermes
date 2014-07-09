using System;
using System.Data.Entity;

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
            database.ExecuteSqlCommand(String.Format(QueueSqlCommands.CreateQueue, queueName.ToUriSafeString()));
        }
    }
}