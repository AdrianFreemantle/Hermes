using System;
using System.Data;

using Hermes.Messaging;

using ServiceStack.OrmLite;

namespace Hermes.ServiceStack
{
    public class OrmLiteProcessManagerPersister : IPersistProcessManagers
    {
        private readonly OrmLiteUnitOfWork unitOfWork;
        private readonly IDbConnectionFactory connectionFactory;

        public OrmLiteProcessManagerPersister(OrmLiteUnitOfWork unitOfWork, IDbConnectionFactory connectionFactory)
        {
            this.unitOfWork = unitOfWork;
            this.connectionFactory = connectionFactory;
        }

        public void Update<T>(T t) where T : class, IContainProcessManagerData, new()
        {
            unitOfWork.AddAction(connection =>
            {
                int oldVersion = t.Version - 1;
                int rowsAffected = connection.Update(t, s => s.Id == t.Id && s.Version == oldVersion);

                if (rowsAffected != 1)
                {
                    throw new DBConcurrencyException(String.Format("{0}:{1} has been modified since it was last read from the database.", t.GetType().Name, t.Id));
                }
            });
        }

        public void Create<T>(T t) where T : class, IContainProcessManagerData, new()
        {
            unitOfWork.AddAction(connection => connection.Insert(t));
        }

        public void Complete<T>(Guid t) where T : class, IContainProcessManagerData, new()
        {
            unitOfWork.AddAction(connection => connection.DeleteById<T>(t));
        }

        public T Get<T>(Guid processId) where T : class, IContainProcessManagerData, new()
        {
            try
            {
                return connectionFactory.Run(c => c.GetById<T>(processId));
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }
    }
}
