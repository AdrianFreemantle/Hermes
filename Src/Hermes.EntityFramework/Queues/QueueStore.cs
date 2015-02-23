using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace Hermes.EntityFramework.Queues
{
    public class QueueStore 
    {
        private readonly EntityFrameworkUnitOfWork unitOfWork;

        public QueueStore(EntityFrameworkUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public Guid Deque(string queueName)
        {
            Mandate.ParameterNotNullOrEmpty(queueName, "queueName");

            Database database = unitOfWork.GetDatabase();

            var command = String.Format(QueueSqlCommands.Dequeue, queueName.ToUriSafeString());
            return database.ExecuteScalarCommand<Guid>(command);
        }

        public bool TryDeque(string queueName, Guid id)
        {
            Mandate.ParameterNotNullOrEmpty(queueName, "queueName");
            Mandate.ParameterNotDefaut(id, "id");

            Database database = unitOfWork.GetDatabase();
            
            int affectedRows = database.ExecuteSqlCommand(String.Format(QueueSqlCommands.Remove, queueName.ToUriSafeString()), new SqlParameter("Id", id));

            return affectedRows > 0;
        }

        public void Remove(string queueName, Guid id)
        {
            if(TryDeque(queueName, id))
                return;

            throw new DBConcurrencyException(String.Format("The task with Id {0} has already been claimed from queue {1}", id, queueName.ToUriSafeString()));
        }

        public void Enqueue(string queueName, Guid id)
        {
            Enqueue(queueName, id, 0);
        }

        public void Enqueue(string queueName, Guid id, int priority)
        {
            Mandate.ParameterNotNullOrEmpty(queueName, "queueName");

            Database database = unitOfWork.GetDatabase();
            database.ExecuteSqlCommand(String.Format(QueueSqlCommands.Enqueue, queueName.ToUriSafeString()), new SqlParameter("Id", id), new SqlParameter("Priority", priority));
        }
    }
}

