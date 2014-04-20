using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using Hermes.Messaging;

namespace Hermes.EntityFramework.ProcessManagager
{
    public class ProcessManagerPersister : IPersistProcessManagers
    {
        private readonly EntityFrameworkUnitOfWork unitOfWork;

        public ProcessManagerPersister(EntityFrameworkUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            unitOfWork.BeginTransaction(IsolationLevel.Serializable);
        }

        public T Find<T>(Expression<Func<T, bool>> expression) where T : class, IContainProcessManagerData, new()
        {
            var repository = unitOfWork.GetRepository<T>();
            return repository.FirstOrDefault(expression);
        }

        public void Create<T>(T state) where T : class, IContainProcessManagerData, new()
        {
            var repository = unitOfWork.GetRepository<T>();
            repository.Add(state);
        }

        public T Get<T>(Guid processId) where T : class, IContainProcessManagerData, new()
        {
            var repository = unitOfWork.GetRepository<T>();
            return repository.Get(processId);
        }

        public void Complete<T>(Guid processId) where T : class, IContainProcessManagerData, new()
        {
            var repository = unitOfWork.GetRepository<T>();
            var entity = repository.Get(processId);
            repository.Remove(entity);
        }

        public void Update<T>(T state) where T : class, IContainProcessManagerData, new()
        {
        }
    }
}
