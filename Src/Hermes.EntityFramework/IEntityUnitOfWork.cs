using Hermes.Messaging;
using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    public interface IEntityUnitOfWork : IUnitOfWork
    {
        EntityFrameworkRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}