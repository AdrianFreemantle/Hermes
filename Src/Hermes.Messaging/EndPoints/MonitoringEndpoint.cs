using Hermes.Ioc;
using Hermes.Messaging.Monitoring.DomainEvents;
using Hermes.Messaging.Monitoring.Pipeline;
using Hermes.Messaging.Monitoring.SystemEvents;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Pipeline.Modules;
using Hermes.Pipes;

namespace Hermes.Messaging.EndPoints
{
    public abstract class MonitoringEndpoint<TContainerBuilder> : ControlEndpoint<TContainerBuilder> 
        where TContainerBuilder : IContainerBuilder, new()
    {
        protected override void ConfigurePipeline(TContainerBuilder containerBuilder)
        {
            var incomingPipeline = new ModulePipeFactory<IncomingMessageContext>()
                .Add<EnqueuedMessageSenderModule>()
                .Add<HeartbeatMonitorModule>()
                .Add<PerformanceMonitorModule>()
                .Add<DispatchMessagesModule>();

            containerBuilder.RegisterSingleton(incomingPipeline);

            HeartbeatMonitorModule.OnEndpointHeartbeatStopped += OnEndpointHeartbeatStopped;
            HeartbeatMonitorModule.OnEndpointHeartbeatResumed += OnEndpointHeartbeatResumed;
            PerformanceMonitorModule.OnEndpointPerformanceMetricReceived += OnEndpointPerformanceMetricReceived;
        }

        private void OnEndpointPerformanceMetricReceived(EndpointPerformanceEventArgs e, object sender)
        {
            IDomainEvent metricReceived = new HermesEndpointPerformanceMetricReceived(e);
            RaiseEvent(metricReceived);
        }

        private void OnEndpointHeartbeatStopped(EndpointHeartbeatStoppedEventArgs e, object sender)
        {
            IDomainEvent endpointFailed = new HermesEndpointFailed(e);
            RaiseEvent(endpointFailed);
        }

        private void OnEndpointHeartbeatResumed(EndpointHeartbeatResumedEventArgs e, object sender)
        {
            IDomainEvent endpointFailed = new HermesEndpointResumed(e);
            RaiseEvent(endpointFailed);
        }
    }
}