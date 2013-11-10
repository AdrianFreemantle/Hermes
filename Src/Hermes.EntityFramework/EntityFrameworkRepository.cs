using System;
using System.Data.Entity;

using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    public class EntityFrameworkRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly IDbSet<TEntity> DbSet;

        internal EntityFrameworkRepository(DbContext context)
        {
            DbSet = context.Set<TEntity>();
        }

        public TEntity Get(int id)
        {
            return DbSet.Find(id);
        }

        public TEntity Get(long id)
        {
            return DbSet.Find(id);
        }

        public TEntity Get(uint id)
        {
            return DbSet.Find(id);
        }

        public TEntity Get(ulong id)
        {
            return DbSet.Find(id);
        }

        public TEntity Get(Guid id)
        {
            return DbSet.Find(id);
        }

        public TEntity Get(string id)
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
    }
}
