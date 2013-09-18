using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDomain.Infrastructure
{
    public interface IRepository<TEntity> : IQueryable<TEntity> where TEntity : class
    {
        TEntity Get(dynamic id);
        void Add(TEntity newEntity);
        void Remove(TEntity entity);
    }

    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}
