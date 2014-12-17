using System.Collections.Generic;
using System.Linq;
using EntityFrameworkTest.Queries.DyanamicCompanyQueries;
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

            var endpoint = new Endpoint();
            endpoint.Start();

            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                var dynamicCompanyQuery = scope.GetInstance<DynamicCompanyQueryService>();

                dynamic google = dynamicCompanyQuery.FetchSingle(company => company.Name == "Google");

                IEnumerable<object> blah = dynamicCompanyQuery.FetchAll(company => company.Employees.Count >= 3);

                var sandraComapny = dynamicCompanyQuery.FetchFirst(company => company.Employees.Any(employee => employee.Name.Contains("Sandra")));

            }
        }
    }
}
