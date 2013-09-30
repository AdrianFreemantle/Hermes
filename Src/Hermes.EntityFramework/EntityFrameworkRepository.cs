using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.EntityFramework
{
    public class EntityFrameworkRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly IDbSet<TEntity> DbSet;

        public Expression Expression { get { return DbSet.Expression; } }
        public Type ElementType { get { return DbSet.ElementType; } }
        public IQueryProvider Provider { get { return DbSet.Provider; } }

        internal EntityFrameworkRepository(IDbSet<TEntity> dbSet)
        {
            this.DbSet = dbSet;
        }

        public TEntity Get(dynamic id)
        {
            return DbSet.Find(id);
        }

        public void Add(TEntity newEntity)
        {
            DbSet.Add(newEntity);
        }

        public void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
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
