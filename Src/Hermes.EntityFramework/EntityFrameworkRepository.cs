using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Linq;

namespace Hermes.EntityFramework
{
    public class EntityFrameworkRepository<TEntity> : IQueryableRepository<TEntity> where TEntity : class
    {
        protected readonly IDbSet<TEntity> DbSet;

        public Expression Expression { get { return DbSet.Expression; } }
        public Type ElementType { get { return DbSet.ElementType; } }
        public IQueryProvider Provider { get { return DbSet.Provider; } }

        internal EntityFrameworkRepository(DbContext context)
        {
            DbSet = context.Set<TEntity>();
        }
   
        public TEntity Get(object id)
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
            return DbSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
