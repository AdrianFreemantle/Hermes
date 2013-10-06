using System;
using System.Collections.Generic;

using Hermes.Ioc;
using Hermes.Messaging;

namespace Hermes.Configuration
{
    /// <summary>
    /// Used to store all infrastructure configuration settings
    /// </summary>
    public static class Settings
    {
        private static readonly Dictionary<string,object> settings = new Dictionary<string, object>();

        private static IContainerBuilder builder;
        private static Address auditAddress = Address.Undefined;
        private static Address errorAddress = Address.Undefined;
        private static Address defermentEndpoint = Address.Parse("Deferment");

        private static int firstLevelRetryAttempts = 3;
        private static TimeSpan firstLevelRetryDelay = TimeSpan.FromMilliseconds(50);
        private static int secondLevelRetryAttempts = 3;
        private static TimeSpan secondLevelRetryDelay = TimeSpan.FromSeconds(15);

        private static int numberOfWorkers = 1;

        public static int NumberOfWorkers
        {
            get { return numberOfWorkers; }
            internal set { numberOfWorkers = value; }
        }

        public static bool UseDistributedTransaction { get; internal set; }

        public static IContainerBuilder Builder
        {
            get
            {
                if (builder == null)
                    throw new InvalidOperationException("Please add a call to Configure.DefaultBuilder() or any of the other supported builders to set one up");

                return builder;
            }

            internal set { builder = value; }
        } 

        public static IContainer RootContainer
        {
            get
            {
                if (builder == null)
                    throw new InvalidOperationException("Please add a call to Configure.DefaultBuilder() or any of the other supported builders to set one up");

                return builder.Container;
            } 
        }

        public static int SecondLevelRetryAttempts
        {
            get { return secondLevelRetryAttempts; }
            internal set { secondLevelRetryAttempts = value; }
        }

        public static TimeSpan SecondLevelRetryDelay
        {
            get { return secondLevelRetryDelay; }
            internal set { secondLevelRetryDelay = value; }
        }

        public static int FirstLevelRetryAttempts
        {
            get { return firstLevelRetryAttempts; }
            internal set { firstLevelRetryAttempts = value; }
        }

        public static TimeSpan FirstLevelRetryDelay
        {
            get { return firstLevelRetryDelay; }
            internal set { firstLevelRetryDelay = value; }
        }

        public static Address DefermentEndpoint
        {
            get { return defermentEndpoint; }
            internal set { defermentEndpoint = value; }
        }

        public static Address ErrorEndpoint
        {
            get { return errorAddress; }
        }

        public static IMessageBus MessageBus
        {
            get { return builder.Container.GetInstance<IMessageBus>(); }
        }

        public static IManageSubscriptions Subscriptions
        {
            get { return builder.Container.GetInstance<IManageSubscriptions>(); }
        }

        public static bool IsSendOnlyEndpoint { get; internal set; }

        internal static void SetEndpointName(string endpointName)
        {
            Address.InitializeLocalAddress(endpointName);
            errorAddress = Address.Local.SubScope("Error");            
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