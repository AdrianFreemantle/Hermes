using System;
using System.Linq;

namespace Hermes.EntityFramework
{
    public interface IRepository<TEntity> : IQueryable<TEntity> where TEntity : class
    {
        TEntity Get(int id);
        TEntity Get(long id);
        TEntity Get(uint id);
        TEntity Get(ulong id);
        TEntity Get(Guid id);
        TEntity Get(string id);
        void Add(TEntity newEntity);
        void Remove(TEntity entity);
    }
}