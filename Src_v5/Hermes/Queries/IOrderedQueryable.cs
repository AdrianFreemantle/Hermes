using System;
using System.Linq.Expressions;

namespace Hermes.Queries
{
    public interface IOrderedQueryable<TEntity, TResult, TDto> : IQueryable<TEntity, TResult, TDto>
        where TEntity : class, new()
        where TDto : class, new()
        where TResult : class, new()
    {
        IOrderedQueryable<TEntity, TResult, TDto> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);
        IOrderedQueryable<TEntity, TResult, TDto> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);
    }
}