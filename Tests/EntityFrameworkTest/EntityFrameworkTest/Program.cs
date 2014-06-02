using System.Collections.Generic;
using System.Linq;
using EntityFrameworkTest.Queries.ComanyDtoQueries;
using EntityFrameworkTest.Queries.DyanamicCompanyQueries;
using EntityFrameworkTest.Queries.EmployeeDtoQueries;
using Hermes.EntityFramework;
using Hermes.EntityFramework.Queries;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Queries;

namespace EntityFrameworkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;
            DatabaseQuery.EnableDebugTrace = true;
            EntityFrameworkUnitOfWork.EnableDebugTrace = true;

            var endpoint = new Endpoint();
            endpoint.Start();

            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                var dynamicCompanyQuery = scope.GetInstance<DynamicCompanyQueryService>();

                dynamic google = dynamicCompanyQuery.FetchSingle(company => company.Name == "Google");

                List<dynamic> companiesMoreThanThreeEmps = dynamicCompanyQuery.FetchAll(company => company.Employees.Count >= 3);

                var sandraComapny = dynamicCompanyQuery.FetchFirst(company => company.Employees.Any(employee => employee.Name.Contains("Sandra")));
            }
        }
    }
}
