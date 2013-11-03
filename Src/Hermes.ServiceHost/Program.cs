using System;
using System.Reflection;

using Hermes.Logging;
using Hermes.Reflection;

using Topshelf;

namespace Hermes.ServiceHost
{
    public class Program
    {
        static readonly ILog Logger;

        static Program()
        {
            if (Environment.UserInteractive)
            {
                LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
                ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Verbose;
                Logger = LogFactory.BuildLogger(typeof(Program));
            }
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            Logger.Info("Starting service host {0}", Assembly.GetEntryAssembly().GetName().FullName);

            TopshelfExitCode exitCode = HostFactory.BuildHost().Run();

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
