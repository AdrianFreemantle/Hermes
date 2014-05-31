using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EntityFrameworkTest.Model;
using Hermes.EntityFramework.Queries;
using Hermes.Queries;

namespace EntityFrameworkTest.Queries
{
    public class FetchAllEmployeesForCompany : IReturn<EmployeeDto[]>
    {
        public string CompanyName { get; set; }
    }

    public class FetchEmployeeWithName: IReturn<EmployeeDto>
    {
        public string Name { get; set; }
    }
    
    public class FetchEmployeesWithNameLike : IReturn<PagedResult<EmployeeDto>>
    {
        public string Name { get; set; }
    }

    public class FetchFirstEmployeeWithNameLike : IReturn<EmployeeDto>
    {
        public string Name { get; set; }
    }

    public class EmployeeQuery : 
        EntityQuery<Employee,EmployeeDto>,
        IAnswerQuery<FetchAllEmployeesForCompany, EmployeeDto[]>,
        IAnswerQuery<FetchEmployeeWithName, EmployeeDto>,
        IAnswerQuery<FetchEmployeesWithNameLike, PagedResult<EmployeeDto>>,
        IAnswerQuery<FetchFirstEmployeeWithNameLike, EmployeeDto>
    {
        public EmployeeQuery(DatabaseQuery databaseQuery) : 
            base(databaseQuery)
        {
            
        }

        protected override IQueryable<Employee> QueryWrapper(IQueryable<Employee> query)
        {
            return query.Include(e => e.Company);
        }

        protected override Expression<Func<Employee, EmployeeDto>> MappingSelector()
        {
            return employee => new EmployeeDto
            {
                Name = employee.Name,
                Company = employee.Company.Name, 
            };
        }

        public EmployeeDto[] Answer(FetchAllEmployeesForCompany query)
        {
            return FetchAll(employee => employee.Company.Name == query.CompanyName).ToArray();
        }

        public EmployeeDto Answer(FetchEmployeeWithName query)
        {
            return FetchSingle(employee => employee.Name == query.Name);
        }

        public PagedResult<EmployeeDto> Answer(FetchEmployeesWithNameLike query)
        {
            return FetchPage(1, queryPredicate: e => e.Name.Contains(query.Name), orderBy: e => e.Name);
        }


        public EmployeeDto Answer(FetchFirstEmployeeWithNameLike query)
        {
            return FetchFirst(employee => employee.Name.Contains(query.Name));
        }
    }
}