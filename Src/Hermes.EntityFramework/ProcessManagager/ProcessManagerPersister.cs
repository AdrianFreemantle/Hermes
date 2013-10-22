using System;
using System.Text;

using Hermes.Messaging.Saga;
using Hermes.Serialization;

namespace Hermes.EntityFramework.ProcessManagager
{
    public class ProcessManagerPersister : IPersistProcessManagers
    {
        private readonly Encoding encoding = Encoding.UTF8;

        private readonly IEntityUnitOfWork unitOfWork;
        private readonly ISerializeObjects serializer;

        public ProcessManagerPersister(IEntityUnitOfWork unitOfWork, ISerializeObjects serializer)
        {
            this.unitOfWork = unitOfWork;
            this.serializer = serializer;
        }

        public void Create<T>(T saga) where T : class, IContainProcessManagerData
        {
            var repository = unitOfWork.GetRepository<ProcessManagerEntity>();

            var entity = new ProcessManagerEntity
            {
                Id = saga.Id,
                State = Serialize(saga)
            };

            repository.Add(entity);
        }

        public void Update<T>(T saga) where T : class, IContainProcessManagerData
        {
            var repository = unitOfWork.GetRepository<ProcessManagerEntity>();
            ProcessManagerEntity entity = repository.Get(saga.Id);
            entity.State = Serialize(saga);
        }

        public T Get<T>(Guid sagaId) where T : class, IContainProcessManagerData
        {
            var repository = unitOfWork.GetRepository<ProcessManagerEntity>();
            var entity = repository.Get(sagaId);

            if (entity == null)
            {
                return null;
            }

            return Deserialize<T>(entity.State);
        }

        public void Complete(Guid sagaId)
        {
            var repository = unitOfWork.GetRepository<ProcessManagerEntity>();
            var entity = repository.Get(sagaId);
            repository.Remove(entity);
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
