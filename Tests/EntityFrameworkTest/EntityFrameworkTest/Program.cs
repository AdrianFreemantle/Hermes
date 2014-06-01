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
                var dtoCompanyQuery = scope.GetInstance<DtoCompanyQueryService>();
                var dtoEmployeeQueryService = scope.GetInstance<DtoEmployeeQueryService>();

                dynamic google = dynamicCompanyQuery.FetchAll(company => company.Name == "Google").Single();
                CompanyDto amazon = dtoCompanyQuery.FetchAll(company => company.Name == "Amazon").Single();
                EmployeeDto billy = dtoEmployeeQueryService.FetchAll(employee => employee.Name == "Billy Bob").Single();

                EmployeeDto[] empDtoAll = dtoEmployeeQueryService.Answer(new FetchAllEmployeesForCompany
                {
                    CompanyName = "Google"
                });

                EmployeeDto empDtoSingle = dtoEmployeeQueryService.Answer(new FetchEmployeeWithName
                {
                    Name = "Billy Bob"
                });

                PagedResult<EmployeeDto> empDtoPage = dtoEmployeeQueryService.Answer(new FetchEmployeesWithNameLike
                {
                    Name = "Smith"
                });

                EmployeeDto empDtoFirst = dtoEmployeeQueryService.Answer(new FetchFirstEmployeeWithNameLike
                {
                    Name = "Smith"
                });
            }
        }
    }
}
