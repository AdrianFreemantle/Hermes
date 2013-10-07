using System.Collections.Generic;

namespace Hermes.EntityFramework.Queries
{
    public interface IDatabaseQuery
    {
        IEnumerable<TDto> SqlQuery<TDto>(string sqlQuery, params object[] parameters);
    }
}