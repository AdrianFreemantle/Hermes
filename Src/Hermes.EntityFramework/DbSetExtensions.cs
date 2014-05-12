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
            return source.Local.FirstOrDefault(predicate) ?? source.FirstOrDefault(predicate) ?? source.Create();
        }

        public static TEntity SingleOrCreate<TEntity>(this IDbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class
        {
            return source.Local.SingleOrDefault(predicate) ?? source.SingleOrDefault(predicate) ?? source.Create();
        }

        public static TEntity FirstOrDefault<TEntity>(this IDbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class
        {
            return source.Local.FirstOrDefault(predicate) ?? source.FirstOrDefault(predicate);
        }

        public static TEntity SingleOrDefault<TEntity>(this IDbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class
        {
            return source.Local.SingleOrDefault(predicate) ?? source.SingleOrDefault(predicate);
        }

        public static TEntity First<TEntity>(this IDbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class
        {
            return source.Local.FirstOrDefault(predicate) ?? source.First(predicate);
        }

        public static TEntity Single<TEntity>(this IDbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class
        {
            return source.Local.SingleOrDefault(predicate) ?? source.Single(predicate);
        }
    }
}