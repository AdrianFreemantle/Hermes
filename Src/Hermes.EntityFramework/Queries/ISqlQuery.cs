using System.Collections.Generic;

namespace Hermes.EntityFramework.Queries
{
    public interface ISqlQuery
    {
        IEnumerable<TDto> SqlQuery<TDto>(string sqlQuery, params object[] parameters);
    }
}