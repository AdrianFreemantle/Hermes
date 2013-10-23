using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.EntityFramework.Queries
{
    public interface ISqlQuery
    {
        SqlQueryResult<TDto> SqlQuery<TDto>(string sqlQuery, params object[] parameters);
    }

    public class SqlQueryResult<T> : IReadOnlyCollection<T>, IQueryable<T>
    {
        private const string NotSupported = "IQueryable methods are not supported for SqlQuery results. Please provide parameter based filters in order to retrieve the desired result set.";
        readonly List<T> data = new List<T>();

        public int Count { get { return data.Count; } }
        public Expression Expression { get { throw new NotSupportedException(NotSupported); } }
        public Type ElementType { get { throw new NotSupportedException(NotSupported); } }
        public IQueryProvider Provider { get { throw new NotSupportedException(NotSupported); } }

        public SqlQueryResult(IEnumerable<T> data)
        {
            this.data = data.ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}