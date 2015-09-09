using System;
using System.Linq;
using EntityFrameworkTest.Model;
using EntityFrameworkTest.Queries.ComanyDtoQueries;
using EntityFrameworkTest.Queries.DyanamicCompanyQueries;
using EntityFrameworkTest.Queries.EmployeeDtoQueries;
using Hermes.EntityFramework.Queries;
using Hermes.EntityFramework.Queues;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

namespace EntityFrameworkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;

            var endpoint = new Endpoint();
            endpoint.Start();

            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                var dtoCompanyQueryService = scope.GetInstance<DtoCompanyQueryService>();
                
                var dtoEmployeeQueryService = scope.GetInstance<DtoEmployeeQueryService>();

                var googlePage = dtoCompanyQueryService
                    .Query
                    .Where(c => c.Employees.Any(employee => employee.Name.Contains("Smith")))
                    .OrderBy(company => company.Name).FetchPage(1, 10);

                var firstCompanyDto = dtoCompanyQueryService.Query.First();
                var companiesWithMoreThanTwoEmployees = dtoCompanyQueryService.Query.Where(company => company.Employees.Count > 2).ToArray();
                var companyQueryService = scope.GetInstance<CompanyQueryService>();

                object[] ordered = companyQueryService.Query
                    .Where(company => company.Employees.Count > 2)
                    .OrderBy(company => company.Employees.Count)
                    .ThenBy(company => company.Name)
                    .ToArray();

                dtoEmployeeQueryService.Query.FirstOrDefault(employee => employee.Name.Length > 2);
            }
        }
    }
}
