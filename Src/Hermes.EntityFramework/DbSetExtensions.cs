using System.Data.Entity;

namespace Hermes.EntityFramework
{
    public static class DbSetExtensions
    {
        public static TEntity Get<TEntity>(this IDbSet<TEntity> dbSet, object key) where TEntity : class
        {
            return dbSet.Find(key);
        }
    }
}