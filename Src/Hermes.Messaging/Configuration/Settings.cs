using System;
using System.Collections.Generic;
using System.Globalization;

using Hermes.Ioc;

namespace Hermes.Messaging.Configuration
{
    /// <summary>
    /// Used to store all infrastructure configuration settings
    /// </summary>
    public static class Settings
    {
        private const string EndpointNameSpace = ".Endpoint";

        private static readonly Dictionary<string,object> settings = new Dictionary<string, object>();

        private static readonly Address ErrorAddress = Address.Parse("Error");
        private static readonly Address AuditAddress = Address.Parse("Audit");
        private static bool autoSubscribeEvents = true;
        private static IContainer rootContainer;
        private static TimeSpan secondLevelRetryDelay = TimeSpan.FromSeconds(50);

        private static int numberOfWorkers = 1;

        static Settings()
        {
            SecondLevelRetryAttempts = 0;
            FirstLevelRetryAttempts = 0;

            IsMessageType = type => false;
            IsCommandType = type => false;
            IsEventType = type => false;
            UseDistributedTransaction = true;
        }        

        public static bool UseDistributedTransaction { get; internal set; }
        public static bool FlushQueueOnStartup { get; internal set; }
        public static bool IsSendOnly { get; internal set; }
        internal static int SecondLevelRetryAttempts { get; set; }
        public static int FirstLevelRetryAttempts { get; internal set; }
        public static bool IsClientEndpoint { get; internal set; }
        internal static Func<Type, bool> IsMessageType { get; set; }
        internal static Func<Type, bool> IsCommandType { get; set; }
        internal static Func<Type, bool> IsEventType { get; set; }
        internal static Func<Guid> UserIdResolver { get; set; }

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