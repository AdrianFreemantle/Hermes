using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
            return Includes(queryable).Select(Selector()).Select(Mapper());
        }

        private IEnumerable<TResult> ExecuteQuery(IQueryable<TEntity> query)
        {
            return Includes(query).Select(Selector()).Select(Mapper());
        }

        private IEnumerable<TResult> ExecuteQuery(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return Includes(queryable.Where(queryPredicate)).Select(Selector()).Select(Mapper());
        }

        private async Task<IEnumerable<TResult>> ExecuteQueryAsync()
        {
            dynamic[] result = await Includes(queryable)
                .Select(Selector())
                .ToArrayAsync();

            return result.Select(Mapper());
        }

        private async Task<IEnumerable<TResult>> ExecuteQueryAsync(Expression<Func<TEntity, bool>> queryPredicate)
        {
            dynamic[] result = await Includes(queryable.Where(queryPredicate))
                .Select(Selector())
                .ToArrayAsync();

            return result.Select(Mapper());
        }

        private async Task<IEnumerable<TResult>> ExecuteQueryAsync(IQueryable<TEntity> query)
        {
            dynamic[] result = await Includes(query)
                .Select(Selector())
                .ToArrayAsync();

            return result.Select(Mapper());
        }

        public void SetPageSize(int size)
        {
            Mandate.That(size > 0);
            pageSize = size;
        }

        public IEnumerable<TResult> FetchAll()
        {
            return ExecuteQuery();
        }

        public Task<IEnumerable<TResult>> FetchAllAsync()
        {
            return ExecuteQueryAsync();
        }

        public IEnumerable<TResult> FetchAll(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate);
        }

        public Task<IEnumerable<TResult>> FetchAllAsync(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQueryAsync(queryPredicate);
        }

        public TResult FetchSingle(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate).Single();
        }

        public async Task<TResult> FetchSingleAsync(Expression<Func<TEntity, bool>> queryPredicate)
        {
            IEnumerable<TResult> result = await ExecuteQueryAsync(queryPredicate);
            return result.Single();
        }

        public TResult FetchSingleOrDefault(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate).SingleOrDefault();
        }

        public async Task<TResult> FetchSingleOrDefaultAsync(Expression<Func<TEntity, bool>> queryPredicate)
        {
            IEnumerable<TResult> result = await ExecuteQueryAsync(queryPredicate);
            return result.SingleOrDefault();
        }

        public TResult FetchFirst(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate).First();
        }

        public async Task<TResult> FetchFirstAsync(Expression<Func<TEntity, bool>> queryPredicate)
        {
            IEnumerable<TResult> result = await ExecuteQueryAsync(queryPredicate);
            return result.First();
        }

        public TResult FetchFirstOrDefault(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return ExecuteQuery(queryPredicate).FirstOrDefault();
        }

        public async Task<TResult> FetchFirstOrDefaultAsync(Expression<Func<TEntity, bool>> queryPredicate)
        {
            IEnumerable<TResult> result = await ExecuteQueryAsync(queryPredicate);
            return result.FirstOrDefault();
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

        public async Task<PagedResult<TResult>> FetchPageAsync<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy)
        {
            return await FetchPageAsync(pageNumber, orderBy, OrderBy.Ascending);
        }

        public async Task<PagedResult<TResult>> FetchPageAsync<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy, OrderBy order)
        {
            var orderedQuery = GetOrderedQuery(orderBy, order)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize);

            var result = await ExecuteQueryAsync(orderedQuery);
            var count = await GetCountAsync();

            return new PagedResult<TResult>(result, pageNumber, pageSize, count);
        }

        public async Task<PagedResult<TResult>> FetchPageAsync<TProperty>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate, Expression<Func<TEntity, TProperty>> orderBy)
        {
            return await FetchPageAsync(pageNumber, queryPredicate, orderBy, OrderBy.Ascending);
        }

        public async Task<PagedResult<TResult>> FetchPageAsync<TProperty>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate, Expression<Func<TEntity, TProperty>> orderBy, OrderBy order)
        {
            var orderedQuery = GetOrderedQuery(orderBy, order)
                .Where(queryPredicate)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize);

            var result = await ExecuteQueryAsync(orderedQuery);
            var count = await GetCountAsync();

            return new PagedResult<TResult>(result, pageNumber, pageSize, count);
        }               

        public int GetCount(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return queryable.Count(queryPredicate);
        }        

        public int GetCount()
        {
            return queryable.Count();
        }

        public Task<int> GetCountAsync()
        {
            return queryable.CountAsync();
        }

        public Task<int> GetCountAsync(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return queryable.CountAsync(queryPredicate);
        }

        public bool Any()
        {
            return queryable.Any();
        }

        public Task<bool> AnyAsync()
        {
            return queryable.AnyAsync();
        }

        public bool Any(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return queryable.Any(queryPredicate);
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return queryable.AnyAsync(queryPredicate);
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

    public abstract class EntityQuery<TEntity> : EntityQuery<TEntity, dynamic>
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