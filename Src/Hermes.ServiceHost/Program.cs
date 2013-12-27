using System;
using System.Configuration;
using System.Reflection;

using Hermes.Logging;

using Topshelf;

namespace Hermes.ServiceHost
{
    public class Program
    {
        private static ILog Logger;
        private static HostableService hostableService;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            ConfigureServiceHost();
            ConfigureLogging();
            RunHostedService();
        }

        private static void ConfigureServiceHost()
        {
            hostableService = HostFactory.GetHostableService();
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", hostableService.GetConfigurationFilePath());
            Configuration c = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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
                LogFactory.BuildLogger = type => new TraceLogger(type);
                TraceLogger.MinimumLogLevel = LogLevel.Debug;
            }

            Logger = LogFactory.BuildLogger(typeof (Program));
        }

        private static void RunHostedService()
        {
            Logger.Info("Starting service host {0}", Assembly.GetEntryAssembly().GetName().FullName);

            TopshelfExitCode exitCode = hostableService.Run();

            if (exitCode == TopshelfExitCode.Ok)
            {
                Logger.Info("Service host terminated normally");
            }
            else
            {
                Logger.Fatal("Service host terminated with error: {0}", exitCode.GetDescription());
            }

            Environment.Exit((int)exitCode);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = ((Exception)e.ExceptionObject);

            if (exception != null)
            {
                Logger.Fatal(String.Format("A fatal error occured while starting the service host: {0}", exception.GetFullExceptionMessage()));
            }
            else
            {
                Logger.Fatal(String.Format("An unknown fatal error occured while starting the service host."));
            }

            if (Environment.UserInteractive)
            {
                Console.ReadKey();
            }

            Environment.FailFast("A fatal error occured while starting the service host", exception);
        }
    }
}
