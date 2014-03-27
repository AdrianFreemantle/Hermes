using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using IntegrationTest.Client.Contracts;

namespace IntegrationTest.Client
{
    class Program
    {
        static readonly int[] Iterations = Enumerable.Range(1, 10000).ToArray();

        static void Main(string[] args)
        {
            var endpoint = new LocalEndpoint();
            endpoint.Start();
            var bus = Settings.RootContainer.GetInstance<IInMemoryBus>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Parallel.ForEach(Iterations, i =>
            {
                bus.Execute(new AddRecordToDatabase(i + 1));
            });

            stopwatch.Stop();
            Console.WriteLine(TimeSpan.FromTicks(stopwatch.ElapsedTicks));
            Console.ReadKey();
        }
    }
}
