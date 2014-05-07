using System;
using System.Linq;

using Hermes.Persistence;

namespace Hermes.EntityFramework
{
    [Obsolete]
    public interface IQueryableRepository<TEntity> : IRepository<TEntity>, IQueryable<TEntity> where TEntity : class
    {
    }
}
