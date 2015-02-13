using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Hermes.EntityFramework.Queries;
using Hermes.Queries;

namespace EntityFrameworkTest.Queries.EmployeeDtoQueries
{
    public class DtoEmployeeQueryService : EntityQuery<Model.Employee,EmployeeDto>
    {
        public DtoEmployeeQueryService(DatabaseQuery databaseQuery) : 
            base(databaseQuery)
        {
            
        }

        protected override IQueryable<Model.Employee> Includes(IQueryable<Model.Employee> query)
        {
            return query.Include(e => e.Company);
        }

        protected override Expression<Func<Model.Employee, object>> Selector()
        {
            return employee => new EmployeeDto
            {
                Name = employee.Name,
                Company = employee.Company.Name, 
            };
        }

        protected override Func<object,EmployeeDto> Mapper()
        {
            return result => (EmployeeDto)result;
        }
    }
}