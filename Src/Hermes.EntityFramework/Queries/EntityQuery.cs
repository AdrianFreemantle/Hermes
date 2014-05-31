using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using Hermes.Logging;
using Hermes.Queries;

namespace Hermes.EntityFramework.Queries
{
    public abstract class EntityQuery<TEntity, TResult>
        where TEntity : class, new()
        where TResult : class, new()
    {

        private readonly IQueryable<TEntity> queryable;
        private int pageSize = 10;


        protected EntityQuery(DatabaseQuery databaseQuery)
        {
            queryable = databaseQuery.GetQueryable<TEntity>();
        }

        protected abstract Expression<Func<TEntity, TResult>> MappingSelector();

        protected virtual IQueryable<TEntity> QueryWrapper(IQueryable<TEntity> query)
        {
            return queryable;
        }

        private IEnumerable<TResult> ExecuteQuery()
        {
            return QueryWrapper(queryable).Select(MappingSelector());
        }

        private IEnumerable<TResult> ExecuteQuery(IQueryable<TEntity> query)
        {
            return QueryWrapper(query).Select(MappingSelector());
        }

        private IEnumerable<TResult> ExecuteQuery(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return QueryWrapper(queryable.Where(queryPredicate)).Select(MappingSelector());
        }

        public void SetPageSize(int size)
        {
            Mandate.That(size > 0);
            pageSize = size;
        }

        public virtual List<TResult> FetchAll()
        {
            return ExecuteQuery().ToList();
        }

        public List<TResult> FetchAll(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate).ToList();
        }

        public TResult FetchSingle(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate).Single();
        }

        public TResult FetchSingleOrDefault(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate).SingleOrDefault();
        }

        public TResult FetchFirst(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate).First();
        }

        public TResult FetchFirstOrDefault(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate).FirstOrDefault();
        }

        public virtual PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy)
        {
            return FetchPage(pageNumber, orderBy, OrderBy.Ascending);
        }

        public virtual PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy, OrderBy order)
        {
            var orderedQuery = GetOrderedQuery(orderBy, order)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize);

            List<TResult> results = ExecuteQuery(orderedQuery).ToList();

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount());
        }

        public virtual PagedResult<TResult> FetchPage<TProperty, TProperty1>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy,
                                                                             Expression<Func<TEntity, TProperty1>> thenOrderBy, OrderBy order)
        {
            var orderedQuery = GetOrderedQuery(orderBy, thenOrderBy, order)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize);

            List<TResult> results = ExecuteQuery(orderedQuery).ToList();

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount());
        }

        public virtual PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate, Expression<Func<TEntity, TProperty>> orderBy)
        {
            return FetchPage(pageNumber, queryPredicate, orderBy, OrderBy.Ascending);
        }

        public virtual PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate, Expression<Func<TEntity, TProperty>> orderBy, OrderBy order)
        {
            var orderedQuery = GetOrderedQuery(orderBy, order)
                .Where(queryPredicate)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize);

            List<TResult> results = ExecuteQuery(orderedQuery).ToList();

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount(queryPredicate));
        }

        public virtual PagedResult<TResult> FetchPage<TProperty, TProperty1>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate,
                                                                             Expression<Func<TEntity, TProperty>> orderBy,
                                                                             Expression<Func<TEntity, TProperty1>> thenOrderBy,
                                                                             OrderBy order)
        {
            var orderedQuery = GetOrderedQuery(orderBy, thenOrderBy, order)
                .Where(queryPredicate)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize);

            List<TResult> results = ExecuteQuery(orderedQuery).ToList();

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount(queryPredicate));
        }

        public virtual int GetCount(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return queryable.Count(queryPredicate);
        }

        public virtual int GetCount()
        {
            return queryable.Count();
        }

        public virtual bool Any(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return queryable.Any(queryPredicate);
        }

        public virtual bool Any()
        {
            return queryable.Any();
        }

        private IQueryable<TEntity> GetOrderedQuery<TProperty>(Expression<Func<TEntity, TProperty>> orderByExpression, OrderBy order)
        {
            if (order == OrderBy.Ascending)
                return queryable.OrderBy(orderByExpression);

            if (order == OrderBy.Descending)
                return queryable.OrderByDescending(orderByExpression);

            throw new ArgumentException("Unknown order by type.");
        }

        private IQueryable<TEntity> GetOrderedQuery<TProperty, TProperty1>(Expression<Func<TEntity, TProperty>> orderByExpression,
                                                                             Expression<Func<TEntity, TProperty1>> thenOrderByExpression,
                                                                             OrderBy order)
        {
            if (order == OrderBy.Ascending)
                return queryable.OrderBy(orderByExpression).ThenBy(thenOrderByExpression);

            if (order == OrderBy.Descending)
                return queryable.OrderByDescending(orderByExpression).ThenByDescending(thenOrderByExpression);

            throw new ArgumentException("Unknown order by type.");
        }

        private int NumberOfRecordsToSkip(int pageNumber, int selectSize)
        {
            Mandate.ParameterCondition(pageNumber > 0, "pageNumber");
            int adjustedPageNumber = pageNumber - 1; //we adjust for the fact that sql server starts at page 0

            return selectSize * adjustedPageNumber;
        }
    }
}