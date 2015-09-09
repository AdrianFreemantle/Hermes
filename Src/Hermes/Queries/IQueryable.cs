using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Hermes.Queries
{
    public interface IQueryable<TEntity, TResult, TDto>
        where TEntity : class, new()
        where TDto : class, new()
        where TResult : class, new()
    {
        IQueryable<TEntity, TResult, TDto> Where(Expression<Func<TEntity, bool>> predicate);
        IOrderedQueryable<TEntity, TResult, TDto> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);
        IOrderedQueryable<TEntity, TResult, TDto> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);
        TDto First();
        TDto First(Expression<Func<TEntity, bool>> predicate);
        TDto FirstOrDefault();
        TDto FirstOrDefault(Expression<Func<TEntity, bool>> predicate);
        TDto Last(Expression<Func<TEntity, bool>> predicate);
        TDto LastOrDefault();
        TDto LastOrDefault(Expression<Func<TEntity, bool>> predicate);
        TDto Single();
        TDto Single(Expression<Func<TEntity, bool>> predicate);
        TDto SingleOrDefault();
        TDto SingleOrDefault(Expression<Func<TEntity, bool>> predicate);
        bool Any();
        bool Any(Expression<Func<TEntity, bool>> predicate);
        int Count();
        int Count(Expression<Func<TEntity, bool>> predicate);
        TDto[] ToArray();
        List<TDto> ToList();
        PagedResult<TDto> FetchPage(int pageNumber, int pageSize);
    }
}