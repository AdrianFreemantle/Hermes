using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.EntityFramework.Queries
{
    public class EntityQuery<TEntity, TResult> 
        where TEntity : class, new() 
        where TResult : class, new()
    {
        private readonly IQueryable<TEntity> query;
        private int pageSize = 10;

        protected readonly Converter<TEntity, TResult> Converter;

        public EntityQuery(IDataQuery databaseQuery, Func<TEntity, TResult> converter)
        {
            query = databaseQuery.GetQueryable<TEntity>();
            Converter = new Converter<TEntity, TResult>(converter);
        }

        protected virtual IQueryable<TEntity> GetQueryable()
        {
            return query;
        }

        public void SetPageSize(int size)
        {
            Mandate.That(size > 0);
            pageSize = size;
        }

        public virtual List<TResult> FetchAll()
        {
            return GetQueryable()
                .ToList()
                .ConvertAll(Converter);
        }

        public List<TResult> FetchAll(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return GetQueryable()
                .Where(queryPredicate)
                .ToList()
                .ConvertAll(Converter);
        }

        public TResult FetchSingle(Expression<Func<TEntity, bool>> queryPredicate)
        {
            var result = GetQueryable()
                .Where(queryPredicate)
                .Single();

            return Converter.Invoke(result);
        }

        public TResult FetchSingleOrDefault(Expression<Func<TEntity, bool>> queryPredicate)
        {
            var result = GetQueryable()
                .Where(queryPredicate)
                .SingleOrDefault();

            return result != null ? Converter.Invoke(result) : null;
        }

        public TResult FetchFirst(Expression<Func<TEntity, bool>> queryPredicate)
        {
            var result = GetQueryable()
                .Where(queryPredicate)
                .First();

            return Converter.Invoke(result);
        }

        public TResult FetchFirstOrDefault(Expression<Func<TEntity, bool>> queryPredicate)
        {
            var result = GetQueryable()
                .Where(queryPredicate)
                .FirstOrDefault();

            return result != null ? Converter.Invoke(result) : null;
        }

        public virtual PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy)
        {
            return FetchPage(pageNumber, orderBy, OrderBy.Ascending);
        }

        public virtual PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy, OrderBy order)
        {
            List<TResult> results = GetOrderedQuery(orderBy, order)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize)
                .ToList()
                .ConvertAll(Converter);

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount());
        }

        public virtual PagedResult<TResult> FetchPage<TProperty, TProperty1>(int pageNumber, Expression<Func<TEntity, TProperty>> orderBy,
                                                                 Expression<Func<TEntity, TProperty1>> thenOrderBy, OrderBy order)
        {
            List<TResult> results = GetOrderedQuery(orderBy, thenOrderBy, order)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize)
                .ToList()
                .ConvertAll(Converter);

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount());
        }

        public virtual PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate, Expression<Func<TEntity, TProperty>> orderBy)
        {
            return FetchPage(pageNumber, queryPredicate, orderBy, OrderBy.Ascending);
        }

        public virtual PagedResult<TResult> FetchPage<TProperty>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate, Expression<Func<TEntity, TProperty>> orderBy, OrderBy order)
        {
            List<TResult> results = GetOrderedQuery( orderBy, order)
                .Where(queryPredicate)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize)               
                .ToList()
                .ConvertAll(Converter); 

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount(queryPredicate));
        }

        public virtual PagedResult<TResult> FetchPage<TProperty, TProperty1>(int pageNumber, Expression<Func<TEntity, bool>> queryPredicate,
                                                                 Expression<Func<TEntity, TProperty>> orderBy,
                                                                 Expression<Func<TEntity, TProperty1>> thenOrderBy,
                                                                 OrderBy order)
        {
            List<TResult> results = GetOrderedQuery(orderBy, thenOrderBy, order)
                .Where(queryPredicate)
                .Skip(NumberOfRecordsToSkip(pageNumber, pageSize))
                .Take(pageSize)
                .ToList()
                .ConvertAll(Converter);

            return new PagedResult<TResult>(results, pageNumber, pageSize, GetCount(queryPredicate));
        }

        public virtual int GetCount(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return query.Count(queryPredicate);
        }

        public virtual int GetCount()
        {
            return query.Count();
        }

        public virtual bool Any(Expression<Func<TEntity, bool>> queryPredicate)
        {
            return query.Any(queryPredicate);
        }

        public virtual bool Any()
        {
            return query.Any();
        }

        protected IQueryable<TEntity> GetOrderedQuery<TProperty>(Expression<Func<TEntity, TProperty>> orderByExpression, OrderBy order)
        {
            if (order == OrderBy.Ascending)
                return GetQueryable().OrderBy(orderByExpression);

            if (order == OrderBy.Descending)
                return GetQueryable().OrderByDescending(orderByExpression);

            throw new ArgumentException("Unknown order by type.");
        }

        protected IQueryable<TEntity> GetOrderedQuery<TProperty, TProperty1>(Expression<Func<TEntity, TProperty>> orderByExpression,
                                                                 Expression<Func<TEntity, TProperty1>> thenOrderByExpression,
                                                                 OrderBy order)
        {
            if (order == OrderBy.Ascending)
                return GetQueryable().OrderBy(orderByExpression).ThenBy(thenOrderByExpression);

            if (order == OrderBy.Descending)
                return GetQueryable().OrderByDescending(orderByExpression).ThenByDescending(thenOrderByExpression);

            throw new ArgumentException("Unknown order by type.");
        }

        protected int NumberOfRecordsToSkip(int pageNumber, int selectSize)
        {
            Mandate.ParameterCondition(pageNumber > 0, "pageNumber");
            int adjustedPageNumber = pageNumber - 1; //we adjust for the fact that sql server starts at page 0
            
            return selectSize * adjustedPageNumber;
        }
    }
}