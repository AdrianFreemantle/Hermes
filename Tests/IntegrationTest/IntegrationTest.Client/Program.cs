using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

using IntegrationTest.Contracts;

namespace IntegrationTest.Client
{
    class Program
    {
        const int NumberOfMessageToSend = 100000; 

        static void Main(string[] args)
        {
            int[] range = Enumerable.Range(0, NumberOfMessageToSend).ToArray();

            //Thread.Sleep(10000); //give worker time to init database etc

            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Fatal;

            var endpoint =  new RequestorEndpoint();
            endpoint.Start();
            var bus = Settings.RootContainer.GetInstance<IMessageBus>();
            int processorCount = Environment.ProcessorCount;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Parallel.For(0, range.Length,
                new ParallelOptions { MaxDegreeOfParallelism = processorCount }, 
                i => bus.Send(new AddRecordToDatabase(i)));

            stopwatch.Stop();
            Console.WriteLine(TimeSpan.FromTicks(stopwatch.ElapsedTicks));
            Console.ReadKey();
        }
    }
}
