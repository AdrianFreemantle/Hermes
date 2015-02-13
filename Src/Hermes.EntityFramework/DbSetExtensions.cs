using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.EntityFramework
{
    public static class DbSetExtensions
    {
        [Obsolete("Please use the new Get method on IRepository")]
        public static TEntity Get<TEntity>(this IDbSet<TEntity> dbSet, object key) where TEntity : class
        {
            return dbSet.Find(key);
        }

        /// <summary>
        /// Retrurns the first entity that can be found, if no entity is found a new unatached instance  is returned
        /// </summary>
        [Obsolete("Please use the new GetOrCreate method on IRepository")]
        public static TEntity FirstOrCreate<TEntity>(this IDbSet<TEntity> source, Expression<Func<TEntity, bool>> predicate) where TEntity : class, new()
        {
            return source.Local.FirstOrDefault(predicate.Compile()) ?? source.FirstOrDefault(predicate) ?? CreateEntity<TEntity>();
        }

        /// <summary>
        /// Retrurns the first entity that can be found, if no entity is found a new unatached instance  is returned
        /// </summary>
        [Obsolete("Please use the GetOrCreate method on IRepository")]
        public static TEntity SingleOrCreate<TEntity>(this IDbSet<TEntity> source, Expression<Func<TEntity, bool>> predicate) where TEntity : class, new()
        {
            return source.Local.SingleOrDefault(predicate.Compile()) ?? source.SingleOrDefault(predicate) ?? CreateEntity<TEntity>();
        }

        private static TEntity CreateEntity<TEntity>() where TEntity : class, new()
        {
            return new TEntity();
        }
    }
}