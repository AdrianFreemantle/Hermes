using Hermes.Configuration;

namespace Hermes.Transports.SqlServer
{
    public static class SqlMessagingSettings 
    {
        public const string MessagingConnectionStringKey = "Hermes.Transports.SqlServer.ConnectionString";

        public static Configure MessageQueueConnectionString(this Configure config, string connectionString)
        {
            Settings.AddSetting(MessagingConnectionStringKey, connectionString);
            return config;
        }
    }
}
