using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Hermes.Queries
{
    public interface IEntityQuery<TEntity, TResult> 
        where TEntity : class, new()
        where TResult : class, new()
    {
        void SetPageSize(int size);
        IEnumerable<TResult> FetchAll();
        IEnumerable<TResult> FetchAll(Expression<Func<TEntity, bool>> queryPredicate);
        TResult FetchSingle(Expression<Func<TEntity, bool>> queryPredicate);
        TResult FetchSingleOrDefault(Expression<Func<TEntity, bool>> queryPredicate);
        TResult FetchFirst(Expression<Func<TEntity, bool>> queryPredicate);
        TResult FetchFirstOrDefault(Expression<Func<TEntity, bool>> queryPredicate);
        PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy);
        PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy, OrderBy order);

        PagedResult<TResult> FetchPage<TProperty>(int pageNumber,
                                                  Expression<Func<TEntity, bool>> queryPredicate,
                                                  Expression<Func<TEntity, TProperty>> orderBy);

        PagedResult<TResult> FetchPage<TProperty>(int pageNumber,
                                                  Expression<Func<TEntity, bool>> queryPredicate,
                                                  Expression<Func<TEntity, TProperty>> orderBy,
                                                  OrderBy order);


        int GetCount(Expression<Func<TEntity, bool>> queryPredicate);
        int GetCount();
        bool Any(Expression<Func<TEntity, bool>> queryPredicate);
        bool Any();
    }
}