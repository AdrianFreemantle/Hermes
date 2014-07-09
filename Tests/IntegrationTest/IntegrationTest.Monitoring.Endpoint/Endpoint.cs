using System;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Monitoring.Events;
using Hermes.Messaging.Monitoring.Pipeline;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Pipeline.Modules;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Pipes;
using Hermes.Serialization.Json;

namespace IntegrationTest.Monitoring.Endpoint
{
    public class Endpoint : ControlEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureWorker configuration)
        {
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;

            configuration
                .FlushQueueOnStartup(true)
                .FirstLevelRetryPolicy(1)
                .UseJsonSerialization()
                .DisableHeartbeatService()
                .DefineEventAs(t => typeof(IDomainEvent).IsAssignableFrom(t))
                .UseSqlTransport()
                .EndpointName("Monitoring");
        }

        protected override void ConfigurePipeline(AutofacAdapter containerBuilder)
        {
            var incomingPipeline = new ModulePipeFactory<IncomingMessageContext>()
                .Add<EnqueuedMessageSenderModule>()
                .Add<HeartbeatMonitorModule>()
                .Add<PerformanceMonitorModule>()
                .Add<UnitOfWorkModule>()
                .Add<DispatchMessagesModule>();

            containerBuilder.RegisterSingleton(incomingPipeline);

            HeartbeatMonitorModule.OnEndpointHeartbeatStopped += OnEndpointHeartbeatStopped;
            PerformanceMonitorModule.OnEndpointPerformanceMetricReceived += OnEndpointPerformanceMetricReceived;
        }

        private void OnEndpointPerformanceMetricReceived(EndpointPerformanceEventArgs e, object sender)
        {
            IDomainEvent metricReceived = new EndpointPerformanceMetricReceived(e);
            RaiseEvent(metricReceived);
        }

        private void OnEndpointHeartbeatStopped(EndpointHeartbeatStoppedEventArgs e, object sender)
        {
            IDomainEvent endpointFailed = new EndpointFailed(e);
            RaiseEvent(endpointFailed);
        }
    }

    public interface IEndpointFailed : IDomainEvent
    {
        string Endpoint { get; }
        DateTime LastSeen { get; }
    }

    public class EndpointFailed : IEndpointFailed
    {
        public string Endpoint { get; protected set; }
        public DateTime LastSeen { get; protected set; }

        public EndpointFailed(EndpointHeartbeatStoppedEventArgs e)
        {
            Endpoint = e.Endpoint;
            LastSeen = e.LastSeen;
        }
    }

    public interface IEndpointPerformanceMetricReceived : IDomainEvent
    {
        Address Endpoint { get; }
        int TotalMessagesProcessed { get; }
        int TotalErrorMessages { get; }
        TimeSpan AverageTimeToDeliver { get; }
        TimeSpan AverageTimeToProcess { get; }
        TimeSpan MonitoringPeriod { get; }
    }

    public class EndpointPerformanceMetricReceived : IEndpointPerformanceMetricReceived
    {
        public Address Endpoint { get; protected set; }
        public int TotalMessagesProcessed { get; protected set; }
        public int TotalErrorMessages { get; protected set; }
        public TimeSpan AverageTimeToDeliver { get; protected set; }
        public TimeSpan AverageTimeToProcess { get; protected set; }
        public TimeSpan MonitoringPeriod { get; protected set; }

        public EndpointPerformanceMetricReceived(EndpointPerformanceEventArgs e)
        {
            Endpoint = e.Endpoint;
            TotalMessagesProcessed = e.TotalMessagesProcessed;
            TotalErrorMessages = e.TotalErrorMessages;
            AverageTimeToDeliver = e.AverageTimeToDeliver;
            AverageTimeToProcess = e.AverageTimeToProcess;
            MonitoringPeriod = e.MonitoringPeriod;
        }
    }

    public class MonitorLoggingService : 
        IHandleMessage<IEndpointPerformanceMetricReceived>,
        IHandleMessage<IEndpointFailed>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (MonitorLoggingService));

        public void Handle(IEndpointPerformanceMetricReceived e)
        {
            Logger.Info("[Endpoint: {0}] [PROCESSED: {1}] [MSG/S: {1}] [ERROR: {3}] [ATTD: {4} ] [ATTP: {5}]",
                                  e.Endpoint,
                                  e.TotalMessagesProcessed,
                                  (e.TotalMessagesProcessed + e.TotalErrorMessages) / e.MonitoringPeriod.Seconds,
                                  e.TotalErrorMessages,
                                  e.AverageTimeToDeliver.Milliseconds,
                                  e.AverageTimeToProcess.Milliseconds);
        }

        public void Handle(IEndpointFailed e)
        {
            Logger.Error("Endpoint {0} has not been seen since {1} and appears to be down.", e.Endpoint, e.LastSeen.ToDisplayString());
        }
    }
}
