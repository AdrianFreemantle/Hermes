using System.Collections.Generic;
using System.Linq;

namespace Hermes.Queries
{
    public class PagedResult<T>
    {
        private readonly T[] results;
        /// <summary>
        /// The actual results from a query
        /// </summary>
        public T[] Results { get { return results; } }

        /// <summary>
        /// The current page number for this query result
        /// </summary>
        public int CurrentPage { get; private set; }

        /// <summary>
        /// The total number of pages for this query
        /// </summary>
        public int PageCount { get; private set; }

        /// <summary>
        /// The number of results contained in a page
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// The number of results in the current page
        /// </summary>
        public int RowCount { get { return results.Length; } }

        /// <summary>
        /// The number of records found by the query
        /// </summary>
        public int TotalResultCount { get; private set; }

        public PagedResult(IEnumerable<T> results, int currentPage, int pageSize, int totalCount)
        {
            this.results = results.ToArray();
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalResultCount = totalCount;
            SetPageCount(pageSize, totalCount);
        }

        private void SetPageCount(int pageSize, int totalCount)
        {
            PageCount = totalCount / pageSize;

            if ((totalCount % pageSize) > 0)
            {
                PageCount++;
            }
        }
    }
}

