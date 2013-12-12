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
        const int NumberOfMessageToSend = 10000; //10 thousand

        static void Main(string[] args)
        {
            Thread.Sleep(4000); //give worker time to init database etc

            var endpoint =  new RequestorEndpoint();
            endpoint.Start();
            var bus = Settings.RootContainer.GetInstance<IMessageBus>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < NumberOfMessageToSend; i++)
            {
                bus.Send(new AddRecordToDatabase());
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }

            stopwatch.Stop();
            Console.WriteLine(TimeSpan.FromTicks(stopwatch.ElapsedTicks));
            Console.ReadKey();
        }
    }
}
