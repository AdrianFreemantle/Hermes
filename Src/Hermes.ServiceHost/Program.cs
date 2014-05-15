using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Hermes.Failover;
using Hermes.Logging;

using Topshelf;
using log4net.Config;

namespace Hermes.ServiceHost
{
    public class Program
    {
        private static ILog logger;
        private static HostableService hostableService;
        private static Configuration configuration;

        static void Main(string[] args)
        {
            ConfigureServiceHost();
            RunHostedService();
        }

        private static void ConfigureServiceHost()
        {
            CriticalError.DefineCriticalErrorAction(OnCriticalError);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            hostableService = HostFactory.GetHostableService();
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", hostableService.GetConfigurationFilePath());
            configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigureLogging();
        }

        private static void ConfigureLogging()
        {
            if (Environment.UserInteractive)
            {
                LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
                ConsoleWindowLogger.MinimumLogLevel = LogLevel.Debug;
            }
            else
            {
                var configFileInfo = new FileInfo(configuration.FilePath);
                XmlConfigurator.Configure(configFileInfo);
                LogFactory.BuildLogger = type => new Log4NetLogger(type);
            }

            logger = LogFactory.BuildLogger(typeof (Program));
        }

        private static void RunHostedService()
        {
            logger.Info("Starting service host {0} for service {1} : {2}", 
                Assembly.GetEntryAssembly().GetName().Name, 
                hostableService.GetServiceName(), 
                hostableService.GetDescription());

            TopshelfExitCode exitCode = hostableService.Run();

            if (exitCode == TopshelfExitCode.Ok)
            {
                logger.Info("Service host terminated normally");
            }
            else
            {
                logger.Fatal("Service host terminated with error: {0}", exitCode.GetDescription());
            }

            Environment.Exit((int)exitCode);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = ((Exception)e.ExceptionObject);
            OnCriticalError("Hermes Service Host is shutting down due an unhandled exception.", exception);
        }

        private static void OnCriticalError(string message, Exception exception)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine("Hermes Service Host is shutting down due to a fatal error. Press any key to exit.");
                Console.ReadKey();
            }

            Environment.FailFast(String.Format("{0}\n{1}", message, exception.GetFullExceptionMessage()), exception);
        }
    }
}
