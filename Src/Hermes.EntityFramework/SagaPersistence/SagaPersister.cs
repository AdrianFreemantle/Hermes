using System;
using System.Text;

using Hermes.Saga;
using Hermes.Serialization;

namespace Hermes.EntityFramework.SagaPersistence
{
    public class SagaPersister : IPersistSagas
    {
        private readonly Encoding encoding = Encoding.UTF8;

        private readonly IUnitOfWork unitOfWork;
        private readonly ISerializeObjects serializer;

        public SagaPersister(IUnitOfWork unitOfWork, ISerializeObjects serializer)
        {
            this.unitOfWork = unitOfWork;
            this.serializer = serializer;
        }

        public void Create<T>(T saga) where T : class, IContainSagaData
        {
            var repository = unitOfWork.GetRepository<SagaEntity>();

            var entity = new SagaEntity
            {
                Id = saga.Id,
                State = Serialize(saga)
            };

            repository.Add(entity);
        }

        public void Update<T>(T saga) where T : class, IContainSagaData
        {
            var repository = unitOfWork.GetRepository<SagaEntity>();
            SagaEntity entity = repository.Get(saga.Id);
            entity.State = Serialize(saga);
        }

        public T Get<T>(Guid sagaId) where T : class, IContainSagaData
        {
            var repository = unitOfWork.GetRepository<SagaEntity>();
            var entity = repository.Get(sagaId);

            return entity == null ? null : Deserialize<T>(entity.State);
        }

        public void Complete<T>(T saga) where T : class, IContainSagaData
        {
            var repository = unitOfWork.GetRepository<T>();
            repository.Remove(saga);
        }

        protected virtual T Deserialize<T>(byte[] data)
        {
            return serializer.DeserializeObject<T>(encoding.GetString(data));
        }

        protected virtual byte[] Serialize(object data)
        {
            return encoding.GetBytes(serializer.SerializeObject(data));
        }          
    }
}
