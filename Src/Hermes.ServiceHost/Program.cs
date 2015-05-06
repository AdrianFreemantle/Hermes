using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Hermes.Enums;
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
            PrintWelcomeMessage();
            
            Console.WriteLine(@"Configuring service host");
            ConfigureServiceHost();

            Console.WriteLine(@"Configuring logging.");
            ConfigureLogging();
            
            Console.WriteLine(@"Running hosted service.");
            RunHostedService();
        }


        private static void ConfigureServiceHost()
        {
            CriticalError.DefineCriticalErrorAction(OnCriticalError);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;            
            hostableService = HostFactory.GetHostableService();

            Console.WriteLine(@"Setting AppDomain config file to: {0}", hostableService.GetConfigurationFilePath());
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", hostableService.GetConfigurationFilePath());

            configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            
        }

        private static void ConfigureLogging()
        {
            bool useLogFile = false;

            useLogFile = GetUseLogFileSetting();

            if (Environment.UserInteractive && !useLogFile)
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

        private static bool GetUseLogFileSetting()
        {
            var appSettings = (AppSettingsSection)configuration.GetSection("appSettings");

            if (appSettings.Settings.AllKeys.Any(s => s.Equals("hermes:useLogFile")))
            {
                return appSettings.Settings["hermes:useLogFile"].Value == "true";
            }

            return false;
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
            CriticalError.Raise("Hermes Service Host is shutting down due an unhandled exception.", (Exception)e.ExceptionObject);
        }

        private static void OnCriticalError(string message, Exception exception)
        {
            string log = String.Format("{0}\n{1}", message, exception.GetFullExceptionMessage());

            try
            {
                logger.Fatal(log);
            }
            catch
            {
                Console.WriteLine(@"{0}", log);
            }
            finally 
            {
                if (Environment.UserInteractive)
                {
                    Console.WriteLine("\nHermes Service Host is shutting down due to a fatal error. Press any key to exit.");
                    Console.ReadKey();
                    Environment.Exit(-1);
                }

                Environment.FailFast(log, exception);
            }
        }

        private static void PrintWelcomeMessage()
        {
            #if (DEBUG)
            Console.WriteLine(@"Hermes.ServiceHost : Running in DEBUG mode");
            #elif (RELEASE)
            Console.WriteLine(@"Hermes.ServiceHost : Running in RELEASE mode");
            #endif
        }
    }
}
