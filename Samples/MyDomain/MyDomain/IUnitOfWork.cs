using System;

namespace MyDomain
{
    public interface IUnitOfWork : IDisposable 
    {
        void Commit();
        void Rollback();
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}