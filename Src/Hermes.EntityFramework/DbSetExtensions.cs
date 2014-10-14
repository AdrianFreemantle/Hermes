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

        public static TEntity FirstOrCreate<TEntity>(this IDbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class
        {
            return source.Local.FirstOrDefault(predicate) ?? source.FirstOrDefault(predicate) ?? CreateEntity(source);
        }
        
        public static TEntity SingleOrCreate<TEntity>(this IDbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class
        {
            return source.Local.SingleOrDefault(predicate) ?? source.SingleOrDefault(predicate) ?? CreateEntity(source);
        }

        private static TEntity CreateEntity<TEntity>(IDbSet<TEntity> source) where TEntity : class
        {
            var entity = source.Create();
            return source.Attach(entity);
        }
    }
}