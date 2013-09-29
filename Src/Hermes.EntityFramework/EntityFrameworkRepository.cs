using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.EntityFramework
{
    class EntityFrameworkRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly IDbSet<TEntity> dbSet;

        public Expression Expression { get { return dbSet.Expression; } }
        public Type ElementType { get { return dbSet.ElementType; } }
        public IQueryProvider Provider { get { return dbSet.Provider; } }

        internal EntityFrameworkRepository(IDbSet<TEntity> dbSet)
        {
            this.dbSet = dbSet;
        }

        public TEntity Get(dynamic id)
        {
            return dbSet.Find(id);
        }

        public void Add(TEntity newEntity)
        {
            dbSet.Add(newEntity);
        }

        public void Remove(TEntity entity)
        {
            dbSet.Remove(entity);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return ((IEnumerable<TEntity>)Provider.Execute(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Provider.Execute(Expression)).GetEnumerator();
        } 
    }
}
