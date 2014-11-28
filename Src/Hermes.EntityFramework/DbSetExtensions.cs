using System;
using System.Data.Entity;
using System.Linq;

namespace Hermes.EntityFramework
{
    public static class DbSetExtensions
    {
        public static TEntity Get<TEntity>(this IDbSet<TEntity> dbSet, object key) where TEntity : class
        {
            return dbSet.Find(key);
        }

        /// <summary>
        /// Retrurns the first entity that can be found, if no entity is found a new unatached instance  is returned
        /// </summary>
        public static TEntity FirstOrCreate<TEntity>(this IDbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class, new()
        {
            TEntity entity = source.Local.FirstOrDefault(predicate);

            if (entity != null)
                return entity;

            entity = source.FirstOrDefault(predicate);

            if (entity != null)
                return entity;

            return new TEntity();
        }

        /// <summary>
        /// Retrurns the first entity that can be found, if no entity is found a new unatached instance  is returned
        /// </summary>
        public static TEntity SingleOrCreate<TEntity>(this IDbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class, new()
        {
            TEntity entity = source.Local.SingleOrDefault(predicate);

            if(entity != null)
                return entity;

            entity = source.SingleOrDefault(predicate);

            if (entity != null)
                return entity;

            return new TEntity();
        }
    }
}