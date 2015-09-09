using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EntityFrameworkTest.Model;
using Hermes.EntityFramework.Queries;

namespace EntityFrameworkTest.Queries.ComanyDtoQueries
{
    public class CompanyTemp
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Employees { get; set; } 
    }

    public class DtoCompanyQueryService : QueryService<Company, CompanyTemp, CompanyDto>
    {
        protected override IQueryable<Company> Includes(IQueryable<Company> query)
        {
            return query.Include(company => company.Employees);
        }

        protected override Expression<Func<Company, CompanyTemp>> Selector()
        {
            return company => new CompanyTemp
            {
                Id = company.Id,
                Name = company.Name,
                Employees = company.Employees.Select(employee => employee.Name),
            };
        }

        protected override Func<CompanyTemp, CompanyDto> Mapper()
        {
            return o =>
            {
                var companyDto = new CompanyDto
                {
                    Name = o.Name,
                    Employees = o.Employees.Select(BuildEmployedPersonDto).ToArray()
                };

                return companyDto;
            };
        }

        private static EmployedPersonDto BuildEmployedPersonDto(string e)
        {
            return new EmployedPersonDto
            {
                Name = e
            };
        }
    }
}