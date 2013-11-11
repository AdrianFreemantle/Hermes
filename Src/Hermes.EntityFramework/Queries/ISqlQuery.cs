using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.EntityFramework.Queries
{
    public interface ISqlQuery
    {
        List<TDto> SqlQuery<TDto>(string sqlQuery, params object[] parameters);
    }
}