using System.Threading;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using MsmqTest.Contracts;

namespace MsmqTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var endpoint = new Endpoint();
            endpoint.Start();

            var bus = Settings.RootContainer.GetInstance<IMessageBus>();
            long number = 1;

            while (true)
            {
                bus.Send(new PrintNumber(number++));
                Thread.Sleep(1);
            }
        }
    }
}
