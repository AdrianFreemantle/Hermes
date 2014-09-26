using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hermes.Queries;

namespace Hermes.EntityFramework.Queries
{
    public abstract class EntityQuery<TEntity, TResult> : IEntityQuery<TEntity, TResult> 
        where TEntity : class, new()
        where TResult : class, new()
    {
        private readonly IQueryable<TEntity> queryable;
        private int pageSize = 10;

        protected EntityQuery(DatabaseQuery databaseQuery)
        {
            queryable = databaseQuery.GetQueryable<TEntity>();
        }

        protected abstract Expression<Func<TEntity, dynamic>> Selector();

        protected abstract Func<dynamic, TResult> Mapper();

        protected virtual IQueryable<TEntity> Includes(IQueryable<TEntity> query)
        {
            return query;
        }

        private IEnumerable<TResult> ExecuteQuery()
        {
            return Includes(queryable).Select(Selector()).ToArray().Select(Mapper());
        }

        private IEnumerable<TResult> ExecuteQuery(IQueryable<TEntity> query)
        {
            return Includes(query).Select(Selector()).ToArray().Select(Mapper());
        }

        private IEnumerable<TResult> ExecuteQuery(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return Includes(queryable.Where(queryPredicate)).Select(Selector()).ToArray().Select(Mapper());
        }

        public void SetPageSize(int size)
        {
            Mandate.That(size > 0);
            pageSize = size;
        }

        public List<TResult> FetchAll()
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

        public PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy)
        {
            return FetchPage(pageNumber, orderBy, OrderBy.Ascending);
        }

        public PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy, OrderBy order)
        {
            var orderedQuery = GetOrderedQuery(orderBy, order)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize);

            List<TResult> results = ExecuteQuery(orderedQuery).ToList();

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount());
        }

        public PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate, Expression<Func<TEntity, TProperty>> orderBy)
        {
            return FetchPage(pageNumber, queryPredicate, orderBy, OrderBy.Ascending);
        }

        public PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate, Expression<Func<TEntity, TProperty>> orderBy, OrderBy order)
        {
            var orderedQuery = GetOrderedQuery(orderBy, order)
                .Where(queryPredicate)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize);

            List<TResult> results = ExecuteQuery(orderedQuery).ToList();

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount(queryPredicate));
        }

        public int GetCount(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return queryable.Count(queryPredicate);
        }

        public int GetCount()
        {
            return queryable.Count();
        }

        public bool Any(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return queryable.Any(queryPredicate);
        }

        public bool Any()
        {
            return queryable.Any();
        }

        protected IQueryable<TEntity> GetOrderedQuery<TProperty>(Expression<Func<TEntity, TProperty>> orderByExpression, OrderBy order)
        {
            if (order == OrderBy.Ascending)
                return queryable.OrderBy(orderByExpression);

            if (order == OrderBy.Descending)
                return queryable.OrderByDescending(orderByExpression);

            throw new ArgumentException("Unknown order by type.");
        }

        protected int NumberOfRecordsToSkip(int pageNumber, int selectSize)
        {
            Mandate.ParameterCondition(pageNumber > 0, "pageNumber");
            int adjustedPageNumber = pageNumber - 1; //we adjust for the fact that sql server starts at page 0

            return selectSize * adjustedPageNumber;
        }
    }


    public abstract class EntityQuery<TEntity> : EntityQuery<TEntity, object>
        where TEntity : class, new()
    {
        protected EntityQuery(DatabaseQuery databaseQuery)
            :base(databaseQuery)
        {
        }

        protected override Expression<Func<TEntity, dynamic>> Selector()
        {
            return e => e;
        }

        protected override Func<dynamic, object> Mapper()
        {
            return o => o;
        }
    }
}