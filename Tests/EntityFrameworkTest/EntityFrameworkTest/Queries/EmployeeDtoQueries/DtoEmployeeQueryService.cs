using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Hermes.EntityFramework.Queries;
using Hermes.Queries;

namespace EntityFrameworkTest.Queries.EmployeeDtoQueries
{
    public class DtoEmployeeQueryService : 
        EntityQuery<Model.Employee,EmployeeDto>,
        IAnswerQuery<FetchAllEmployeesForCompany, EmployeeDto[]>,
        IAnswerQuery<FetchEmployeeWithName, EmployeeDto>,
        IAnswerQuery<FetchEmployeesWithNameLike, PagedResult<EmployeeDto>>,
        IAnswerQuery<FetchFirstEmployeeWithNameLike, EmployeeDto>
    {
        public DtoEmployeeQueryService(DatabaseQuery databaseQuery) : 
            base(databaseQuery)
        {
            
        }

        protected override IQueryable<Model.Employee> QueryWrapper(IQueryable<Model.Employee> query)
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