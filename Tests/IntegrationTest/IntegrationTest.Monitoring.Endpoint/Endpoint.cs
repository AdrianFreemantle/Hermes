using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

namespace IntegrationTest.Monitoring.Endpoint
{
    public abstract class Endpoint : MonitoringEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureWorker configuration)
        {
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;

            configuration
                .FlushQueueOnStartup(true)
                .UseJsonSerialization()
                .DisableHeartbeatService()
                .DefineEventAs(t => typeof(IDomainEvent).IsAssignableFrom(t))
                .UseSqlTransport()
                .EndpointName("Monitoring");
        }
    }
}
