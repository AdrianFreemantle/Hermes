using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EntityFrameworkTest.Model;
using Hermes.EntityFramework.Queries;

namespace EntityFrameworkTest.Queries.EmployeeDtoQueries
{
    public class DtoEmployeeQueryService : QueryService<Employee, object, EmployeeDto>
    {
        protected override IQueryable<Employee> Includes(IQueryable<Employee> query)
        {
            return query.Include(e => e.Company);
        }

        protected override Expression<Func<Employee, object>> Selector()
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