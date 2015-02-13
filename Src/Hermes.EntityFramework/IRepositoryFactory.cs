using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.EntityFramework
{
    public interface IRepositoryFactory 
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, new();
    }

    public interface IRepository<TEntity> : IDbSet<TEntity> where TEntity : class, new()
    {
        TEntity GetOrCreate(Expression<Func<TEntity, bool>> predicate);
    }

    internal class EntityFrameworkRepository<TEntity> : IRepository<TEntity>  where TEntity : class, new()
    {
        private readonly IDbSet<TEntity> dbSet;

        public ObservableCollection<TEntity> Local { get { return dbSet.Local; }}
        public Expression Expression { get { return dbSet.Expression; } }
        public Type ElementType { get { return dbSet.ElementType; } }
        public IQueryProvider Provider { get { return dbSet.Provider; } }

        public EntityFrameworkRepository(IDbSet<TEntity> dbSet)
        {
            this.dbSet = dbSet;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return dbSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TEntity Get(object key)
        {
            return dbSet.Find(key);
        }

        [Obsolete("To be removed")]
        public TEntity Find(params object[] keyValues)
        {
            return dbSet.Find(keyValues);
        }

        public TEntity Add(TEntity entity)
        {
            return dbSet.Add(entity);
        }

        public TEntity Remove(TEntity entity)
        {
            return dbSet.Remove(entity);
        }

        [Obsolete("To be removed")]
        public TEntity Attach(TEntity entity)
        {
            return dbSet.Attach(entity);
        }

        [Obsolete("To be removed")]
        public TEntity Create()
        {
            return dbSet.Create();
        }

        [Obsolete("To be removed")]
        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
        {
            return dbSet.Create<TDerivedEntity>();
        }

        public TEntity GetOrCreate(Expression<Func<TEntity, bool>> predicate)
        {
            return Local.FirstOrDefault(predicate.Compile()) ?? this.FirstOrDefault(predicate) ?? new TEntity();
        }
    }
}