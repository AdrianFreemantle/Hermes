using System;
using System.Timers;

using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.Monitoring;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

using IntegrationTest.Contracts;

namespace IntegrationTest.Client
{
    public class RequestorEndpoint : ClientEndpoint<AutofacAdapter>
    {
        Timer t = new Timer(TimeSpan.FromMinutes(30).TotalSeconds);
        
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            configuration
                .UseJsonSerialization()
                .UseSqlTransport()
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .RegisterMessageRoute<AddRecordToDatabase>(Address.Parse("IntegrationTest"))
                .NumberOfWorkers(4);

            t.Elapsed += t_Elapsed;
            //t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            //this.Stop();
        }
        
        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type);
        }

        private static bool IsEvent(Type type)
        {
            return typeof(IEvent).IsAssignableFrom(type);
        }
    }
}
