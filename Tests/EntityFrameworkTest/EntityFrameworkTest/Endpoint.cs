using System;
using EntityFrameworkTest.Model;
using Hermes.EntityFramework;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

namespace EntityFrameworkTest
{
    public class Endpoint : LocalEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            ContextFactory<EntityFrameworkTestContext>.DebugTrace = true;
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;
            LogFactory.BuildLogger = t => new ConsoleWindowLogger(t);

            configuration
                .UseJsonSerialization()
                .UserNameResolver(GetCurrentUserName)
                .UseSqlTransport("SqlTransport")
                .DefineCommandAs(IsCommand)
                .ConfigureEntityFramework<EntityFrameworkTestContext>("EntityFrameworkTest");
        }

        private static string GetCurrentUserName()
        {
            return Environment.UserName;
        }
       
        private static bool IsCommand(Type type)
        {
            if (type == null || type.Namespace == null)
                return false;

            return typeof (ICommand).IsAssignableFrom(type);
        }
    }

    public interface ICommand
    {
        
    }

    public class IntitalizeQueue : ICommand
    {
        public string Name { get; set; }
    }




}