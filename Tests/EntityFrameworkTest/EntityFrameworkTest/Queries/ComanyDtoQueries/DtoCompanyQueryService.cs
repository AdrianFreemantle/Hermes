using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EntityFrameworkTest.Model;
using Hermes.EntityFramework.Queries;

namespace EntityFrameworkTest.Queries.ComanyDtoQueries
{
    public class DtoCompanyQueryService : EntityQuery<Company, CompanyDto>
    {
        public DtoCompanyQueryService(DatabaseQuery databaseQuery)
            : base(databaseQuery)
        {
        }

        protected override IQueryable<Company> Includes(IQueryable<Company> query)
        {
            return query.Include(company => company.Employees);
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

        protected override Func<dynamic, CompanyDto> Mapper()
        {
            return o =>
            {
                var companyDto = new CompanyDto
                {
                    Name = o.Name,
                    EmployeeCount = o.Employees.Count,
                };

                var employedPersons = new List<EmployedPersonDto>();

                foreach (string employeeName in o.Employees)
                {
                    employedPersons.Add(new EmployedPersonDto
                    {
                        Name = employeeName
                    });
                }

                companyDto.Employees = employedPersons.ToArray();

                return companyDto;
            };
        }
    }
}