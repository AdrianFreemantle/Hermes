using System;
using System.Linq;
using System.Linq.Expressions;
using EntityFrameworkTest.Model;
using Hermes.EntityFramework.Queries;

namespace EntityFrameworkTest.Queries.DyanamicCompanyQueries
{
    public class DynamicCompanyQueryService : QueryService<Company>
    {
        protected override Expression<Func<Company, object>> Selector()
        {
            return company => new
            {
                company.Id,
                company.Name,
                Employees = company.Employees.Select(employee => employee.Name),
            };

        }

        protected override IQueryable<Company> Includes(IQueryable<Company> query)
        {
            return query;
        }
    }
}
