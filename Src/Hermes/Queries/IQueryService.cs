namespace Hermes.Queries
{
    public interface IQueryService<TEntity, TResult, TDto>
        where TEntity : class, new()
        where TDto : class, new()
        where TResult : class, new()
    {
        Queryable<TEntity, TResult, TDto> Query { get; }
    }
}