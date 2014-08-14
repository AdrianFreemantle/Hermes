using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using LocalBus.Contracts;

namespace LocalBus
{
    class Program
    {
        static readonly int[] Iterations = Enumerable.Range(1, 1000).ToArray();

        static void Main(string[] args)
        {
            var endpoint = new LocalEndpoint();
            endpoint.Start();
            var bus = Settings.RootContainer.GetInstance<IInMemoryBus>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Parallel.ForEach(Iterations, i =>
            {
                try
                {
                    bus.Execute(new AddRecordToDatabase(i + 1));
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                }
            });


            stopwatch.Stop();
            Console.WriteLine(TimeSpan.FromTicks(stopwatch.ElapsedTicks));
            Console.ReadKey();
        }
    }
}
