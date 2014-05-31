using System;
using EntityFrameworkTest.Model;
using EntityFrameworkTest.Queries;
using Hermes.EntityFramework;
using Hermes.EntityFramework.Queries;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Queries;
using Hermes.Serialization.Json;

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
                var queryService = scope.GetInstance<EmployeeQuery>();

                EmployeeDto[] all = queryService.Answer(new FetchAllEmployeesForCompany
                {
                    CompanyName = "Google"
                });

                EmployeeDto single = queryService.Answer(new FetchEmployeeWithName
                {
                    Name = "Billy Bob"
                });

                PagedResult<EmployeeDto> page = queryService.Answer(new FetchEmployeesWithNameLike
                {
                    Name = "Smith"
                });

                EmployeeDto first = queryService.Answer(new FetchFirstEmployeeWithNameLike
                {
                    Name = "Smith"
                });

            }
        }
    }

    public class Endpoint : LocalEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            configuration
                .UseJsonSerialization()
                .UserNameResolver(GetCurrentUserName)
                .UseSqlTransport("SqlTransport")
                .ConfigureEntityFramework<EntityFrameworkTestContext>("EntityFrameworkTest");
        }

        private static string GetCurrentUserName()
        {
            return Environment.UserName;
        }
    }
}
