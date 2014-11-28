using System;
using System.Collections.Generic;
using System.Globalization;
using Hermes.Failover;
using Hermes.Ioc;

namespace Hermes.Messaging.Configuration
{
    /// <summary>
    /// Used to store all infrastructure configuration settings
    /// </summary>
    public static class Settings
    {
        private const string EndpointNameSpace = ".Endpoint";
        private static int numberOfWorkers = 1;
        private static readonly Dictionary<string,object> settings = new Dictionary<string, object>();
        
        private static readonly Address ErrorAddress = Address.Parse("Error");
        private static readonly Address AuditAddress = Address.Parse("Audit");
        private static readonly Address MonitoringAddress = Address.Parse("Monitoring");

        private static bool autoSubscribeEvents = true;
        private static IContainer rootContainer;
        private static TimeSpan secondLevelRetryDelay = TimeSpan.FromSeconds(50);

        public static int SecondLevelRetryAttempts { get; internal set; }
        public static Func<Type, bool> IsMessageType { get; internal set; }
        public static Func<Type, bool> IsCommandType { get; internal set; }
        public static Func<Type, bool> IsEventType { get; internal set; }
        
        public static bool DisableDistributedTransactions { get; internal set; }
        public static bool FlushQueueOnStartup { get; internal set; }
        public static bool IsSendOnly { get; internal set; }
        public static bool IsLocalEndpoint { get; internal set; }
        public static int FirstLevelRetryAttempts { get; internal set; }
        public static bool IsClientEndpoint { get; internal set; }
        public static bool SubsribeToDomainEvents { get; internal set; }
        public static bool DisablePerformanceMonitoring { get; internal set; }
        public static bool DisableHeartbeatService { get; internal set; }
        public static bool DisableMessageAudit { get; set; }
        public static TimeSpan CircuitBreakerReset { get; internal set; }
        public static int CircuitBreakerThreshold { get; internal set; }
        public static bool EnableCommandValidationClasses { get; internal set; }
        public static Func<string> UserNameResolver { get; internal set; }

        static Settings()
        {
            SecondLevelRetryAttempts = 0;
            FirstLevelRetryAttempts = 0;
            CircuitBreakerReset = TimeSpan.FromSeconds(30);
            CircuitBreakerThreshold = 10;

            IsMessageType = type => false;
            IsCommandType = type => false;
            IsEventType = type => false;
        }

        public static IContainer RootContainer
        {
            get
            {
                if (rootContainer == null)
                    throw new InvalidOperationException("IoC container has not been built.");

                return rootContainer;
            }

            internal set { rootContainer = value; }
        }

        public static int NumberOfWorkers
        {
            get { return numberOfWorkers; }
            internal set { numberOfWorkers = value; }
        }

        public static bool AutoSubscribeEvents
        {
            get { return autoSubscribeEvents; }
            internal set { autoSubscribeEvents = value; }
        }        

        public static TimeSpan SecondLevelRetryDelay
        {
            get { return secondLevelRetryDelay; }
            internal set { secondLevelRetryDelay = value; }
        }

        public static Address ErrorEndpoint
        {
            get { return ErrorAddress; }
        }

        public static Address AuditEndpoint
        {
            get { return AuditAddress; }
        }

        public static Address MonitoringEndpoint
        {
            get { return MonitoringAddress; }
        }

        public static IManageSubscriptions Subscriptions
        {
            get { return RootContainer.GetInstance<IManageSubscriptions>(); }
        }

        internal static void SetEndpointName(string endpointName)
        {
            Address.InitializeLocalAddress(ConfigureServiceName(endpointName));         
        }

        private static string ConfigureServiceName(string endpointName)
        {
            if (endpointName.EndsWith(EndpointNameSpace, true, CultureInfo.InvariantCulture))
            {
                return endpointName.Substring(0, endpointName.Length - EndpointNameSpace.Length);
            }

            return endpointName;
        }
 
        public static T GetSetting<T>(string settingKey)
        {
            if (settings.ContainsKey(settingKey))
            {
                return (T)settings[settingKey];
            }

            throw new ConfigurationSettingNotFoundException(settingKey);
        }

        public static void AddSetting(string settingKey, object value)
        {
            if (settings.ContainsKey(settingKey))
            {
                settings.Remove(settingKey);
            }

            settings.Add(settingKey, value);
        }        
    }
}