using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.Queries
{
    public class Queryable<TEntity, TResult, TDto> :
        IOrderedQueryable<TEntity, TResult, TDto>
        where TEntity : class, new()
        where TDto : class, new()
        where TResult : class, new()
    {
        private readonly Expression<Func<TEntity, TResult>> selector;
        private readonly Func<TResult, TDto> mapper;
        private IQueryable<TEntity> queryable;

        public Queryable(IQueryable<TEntity> queryable, Expression<Func<TEntity, TResult>> selector, Func<TResult, TDto> mapper)
        {
            this.queryable = queryable;
            this.selector = selector;
            this.mapper = mapper;
        }

        public IQueryable<TEntity, TResult, TDto> Where(Expression<Func<TEntity, bool>> predicate)
        {
            queryable = queryable.Where(predicate);
            return this;
        }

        public IOrderedQueryable<TEntity, TResult, TDto> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            queryable = queryable.OrderBy(keySelector);
            return this;
        }

        public IOrderedQueryable<TEntity, TResult, TDto> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            queryable = queryable.OrderBy(keySelector);
            return this;
        }

        public IOrderedQueryable<TEntity, TResult, TDto> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            queryable = queryable.OrderBy(keySelector);
            return this;
        }

        public IOrderedQueryable<TEntity, TResult, TDto> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            queryable = queryable.OrderBy(keySelector);
            return this;
        }

        public TDto First()
        {
            var result = queryable.Select(selector).First();
            return mapper(result);
        }

        public TDto First(Expression<Func<TEntity, bool>> predicate)
        {
            var result = queryable.Where(predicate).Select(selector).First();
            return mapper(result);
        }

        public TDto FirstOrDefault()
        {
            var result = queryable.Select(selector).FirstOrDefault();
            return result == null ? null : mapper(result);
        }

        public TDto FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            var result = queryable.Where(predicate).Select(selector).FirstOrDefault();
            return result == null ? null : mapper(result);
        }

        public TDto Last()
        {
            var result = queryable.Select(selector).Last();
            return mapper(result);
        }

        public TDto Last(Expression<Func<TEntity, bool>> predicate)
        {
            var result = queryable.Where(predicate).Select(selector).Last();
            return mapper(result);
        }

        public TDto LastOrDefault()
        {
            var result = queryable.Select(selector).LastOrDefault();
            return result == null ? null : mapper(result);
        }

        public TDto LastOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            var result = queryable.Where(predicate).Select(selector).LastOrDefault();
            return result == null ? null : mapper(result);
        }

        public TDto Single()
        {
            var result = queryable.Select(selector).Single();
            return mapper(result);
        }

        public TDto Single(Expression<Func<TEntity, bool>> predicate)
        {
            var result = queryable.Where(predicate).Select(selector).Single();
            return mapper(result);
        }

        public TDto SingleOrDefault()
        {
            var result = queryable.Select(selector).SingleOrDefault();
            return result == null ? null : mapper(result);
        }

        public TDto SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            var result = queryable.Where(predicate).Select(selector).SingleOrDefault();
            return result == null ? null : mapper(result);
        }

        public bool Any()
        {
            return queryable.Any();
        }

        public bool Any(Expression<Func<TEntity, bool>> predicate)
        {
            return queryable.Any(predicate);
        }

        public int Count()
        {
            return queryable.Count();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return queryable.Count(predicate);
        }

        public TDto[] ToArray()
        {
            return ExecuteQuery(queryable).ToArray();
        }

        public List<TDto> ToList()
        {
            return ExecuteQuery(queryable).ToList();
        }

        private IEnumerable<TDto> ExecuteQuery(IQueryable<TEntity> q)
        {
            return q.Select(selector).Select(mapper);
        }

        public PagedResult<TDto> FetchPage(int pageNumber, int pageSize)
        {
            var skip = NumberOfRecordsToSkip(pageNumber, pageSize);

            var pagedQuery = queryable.Skip(skip)
                .Take(pageSize);

            List<TDto> results = ExecuteQuery(pagedQuery).ToList();
            var count = Count();

            return new PagedResult<TDto>(results, pageNumber, pageSize, count);
        }

        private int NumberOfRecordsToSkip(int pageNumber, int selectSize)
        {
            Mandate.ParameterCondition(pageNumber > 0, "pageNumber");
            int adjustedPageNumber = pageNumber - 1; //we adjust for the fact that sql server starts at page 0

            return selectSize * adjustedPageNumber;
        }
    }
}