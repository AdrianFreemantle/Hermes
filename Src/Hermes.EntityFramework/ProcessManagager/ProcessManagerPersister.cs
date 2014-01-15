using System;
using System.Linq;
using System.Linq.Expressions;

using Hermes.Messaging;

namespace Hermes.EntityFramework.ProcessManagager
{
    public class ProcessManagerPersister : IPersistProcessManagers
    {
        private readonly IRepositoryFactory repositoryFactory;

        public ProcessManagerPersister(IRepositoryFactory repositoryFactory)
        {
            this.repositoryFactory = repositoryFactory;
        }

        public void Create<T>(T state) where T : class, IContainProcessManagerData, new()
        {
            var repository = repositoryFactory.GetRepository<T>();
            repository.Add(state);
        }

        public void Update<T>(T state) where T : class, IContainProcessManagerData, new()
        {
            //no-operation required for entity framework
        }

        public T Get<T>(Guid processId) where T : class, IContainProcessManagerData, new()
        {
            var repository = repositoryFactory.GetRepository<T>();
            return repository.Get(processId);
        }

        public void Complete<T>(Guid processId) where T : class, IContainProcessManagerData, new()
        {
            var repository = repositoryFactory.GetRepository<T>();
            var entity = repository.Get(processId);
            repository.Remove(entity);
        }

        public T Find<T>(Expression<Func<T, bool>> expression) where T : class, IContainProcessManagerData, new()
        {
            var repository = repositoryFactory.GetRepository<T>();
            return repository.FirstOrDefault(expression);
        }
    }
}
