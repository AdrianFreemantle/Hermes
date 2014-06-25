using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using ProcessManagement.Contracts;
using ProcessManagement.Contracts.Commands;

namespace ProcessManagement.Client
{
    public class Endpoint : ClientEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;

            configuration
                .UseJsonSerialization()
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .DefineMessageAs(IsMessage)
                .UseSqlTransport("SqlTransport")
                .RegisterMessageRoute<PayForReservedSeat>(Address.Parse("ProcessManagement"))
                .RegisterMessageRoute<ReserveSeat>(Address.Parse("ProcessManagement"));
        }

        private static bool IsCommand(Type type)
        {
            if (type == null || type.Namespace == null)
                return false;

            return typeof(ICommand).IsAssignableFrom(type);
        }

        private static bool IsEvent(Type type)
        {
            if (type == null || type.Namespace == null)
                return false;

            return typeof(IEvent).IsAssignableFrom(type);
        }

        private static bool IsMessage(Type type)
        {
            if (type == null || type.Namespace == null)
                return false;

            return typeof(IMessage).IsAssignableFrom(type);
        }
    }
}
