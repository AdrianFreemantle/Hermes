using System;
using System.Collections.Generic;
using System.Globalization;
using Clientele.Ocr.Contracts.Events;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

namespace Clientele.Ocr.Fake.Shell
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";
        private static IMessageBus bus;

        static void Main(string[] args)
        {
            Configure
                .Endpoint("Ocr", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .NumberOfWorkers(1)
                .Start();

            bus = Settings.MessageBus;

            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            bool shouldExit = false;

            while (!shouldExit)
            {
                PrintMenu();
                var result = Console.ReadKey();

                switch (result.KeyChar)
                {
                    case '1':
                        FullOcrWorkflow();
                        break;

                    case '2':
                        RecognitionFailedWorkFlow();
                        break;

                    case '3':
                        ExportFailedWorkflow();
                        break;

                    case '4':
                        shouldExit = true;
                        break;
                }
            }
        }

        static void FullOcrWorkflow()
        {
            Guid documentId = Guid.NewGuid();

            var recognitionDone = new RecognitionDone { DocumentId = documentId };

            var verificationDone = new VerificationDone
            {
                DocumentId = documentId,
                VerfiedByUser = "Clientele\\Adrian",
                MetaData = new Dictionary<string, string>
                {
                    {"IfaNumber", GenerateIfaNumber()},
                    {"PolicyNumber", GeneratePolicyNumber()}
                }
            };

            var exportComplete = new ExportCompleted
            {
                DocumentId = documentId,
                FilePath = new Uri(String.Format("ftp://FileExport/ocr/{0}.pdf", documentId))
            };

            bus.Publish(recognitionDone);
            bus.Publish(verificationDone);
            bus.Publish(exportComplete);
        }

        static void RecognitionFailedWorkFlow()
        {
            var recognitionFailed = new RecognitionFailed() { DocumentId = Guid.NewGuid() };
            bus.Publish(recognitionFailed);
        }

        static void ExportFailedWorkflow()
        {
            Guid documentId = Guid.NewGuid();

            var recognitionDone = new RecognitionDone { DocumentId = documentId };

            var verificationDone = new VerificationDone
            {
                DocumentId = documentId,
                VerfiedByUser = "Clientele\\Adrian",
                MetaData = new Dictionary<string, string>
                {
                    {"IfaNumber", GenerateIfaNumber()},
                    {"PolicyNumber", GeneratePolicyNumber()}
                }
            };

            var exportFailed = new ExportFailed
            {
                DocumentId = documentId,
                ErrorMessage = "Ftp server currently unavailable."
            };

            bus.Publish(recognitionDone);
            bus.Publish(verificationDone);
            bus.Publish(exportFailed);
        }

        static string GenerateIfaNumber()
        {
            var ticksString = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            return String.Format("A{0}", ticksString.Substring(0, 6));
        }

        static string GeneratePolicyNumber()
        {
            var ticksString = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            return String.Format("6{0}", ticksString.Substring(0, 8));
        }

        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("Select a scenario");
            Console.WriteLine("=================");
            Console.WriteLine("1. Full OCR workflow");
            Console.WriteLine("2. Recogniction Failed OCR workflow");
            Console.WriteLine("3. Export Failed OCR workflow");
            Console.WriteLine("4. Exit");
            Console.WriteLine("\n>");
        }
    }
}
