using System;
using System.Diagnostics;
using System.Threading;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

using IntegrationTest.Contracts;

namespace IntegrationTest.Client
{
    class Program
    {
        const int NumberOfMessageToSend = 1000000; //10 thousand

        static void Main(string[] args)
        {
            var endpoint =  new RequestorEndpoint();
            endpoint.Start();
            var bus = Settings.RootContainer.GetInstance<IMessageBus>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < NumberOfMessageToSend; i++)
            {
                bus.Send(new AddRecordToDatabase());
                Thread.Sleep(TimeSpan.FromMilliseconds(0.5));
            }

            stopwatch.Stop();
            Console.WriteLine(TimeSpan.FromTicks(stopwatch.ElapsedTicks));
            Console.ReadKey();
        }
    }
}
