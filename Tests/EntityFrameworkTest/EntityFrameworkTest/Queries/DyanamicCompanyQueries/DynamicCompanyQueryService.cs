using System;
using System.Linq;
using System.Linq.Expressions;
using EntityFrameworkTest.Model;
using Hermes.EntityFramework.Queries;

namespace EntityFrameworkTest.Queries.DyanamicCompanyQueries
{
    public class DynamicCompanyQueryService 
        : EntityQuery<Company, dynamic>
    {
        public DynamicCompanyQueryService(DatabaseQuery databaseQuery)
            : base(databaseQuery)
        {
        }

        protected override Expression<Func<Company, object>> Selector()
        {
            return company => new
            {
                company.Id,
                company.Name,
                Employees = company.Employees.Select(employee => employee.Name),
            };

        }

        protected override Func<dynamic, dynamic> Mapper()
        {
            return o => o;
        }
    }
}
