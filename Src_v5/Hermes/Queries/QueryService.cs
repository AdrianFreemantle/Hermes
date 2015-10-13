using System;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.Queries
{
    public abstract class QueryService<TEntity> : QueryService<TEntity, object, object> 
        where TEntity : class, new()
    {
        protected override Func<object, object> Mapper()
        {
            return o => o;
        }
    }

    public abstract class QueryService<TEntity, TResult, TDto> 
        where TEntity : class, new()
        where TDto : class, new()
        where TResult : class, new()
    {
        protected abstract Expression<Func<TEntity, TResult>> Selector();
        protected abstract Func<TResult, TDto> Mapper();
        protected abstract IQueryable<TEntity> Includes(IQueryable<TEntity> query);

        public IDatabaseQuery DatabaseQuery { get; set; }

        public Queryable<TEntity, TResult, TDto> Query
        {
            get
            {
                var queryable = DatabaseQuery.GetQueryable<TEntity>();
                queryable = Includes(queryable);
                return new Queryable<TEntity, TResult, TDto>(queryable, Selector(), Mapper());
            }
        }
    }
}