using System;

namespace Hermes.Persistence
{
    [Obsolete]
    public interface IRepository<TEntity> where TEntity : class 
    {
        TEntity Get(object id);
        TEntity Add(TEntity newEntity);
        TEntity Remove(TEntity entity);
    }
}
