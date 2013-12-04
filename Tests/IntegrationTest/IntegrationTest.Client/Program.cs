using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports.Monitoring;

using IntegrationTest.Contracts;

namespace IntegrationTest.Client
{
    class Program
    {
        const int numberOfMessageToSend = 1000000;
        static void Main(string[] args)
        {
            var endpoint =  new RequestorEndpoint();
            endpoint.Start();
            var bus = Settings.RootContainer.GetInstance<IMessageBus>();

            for (int i = 0; i < numberOfMessageToSend; i++)
            {
                bus.Send(new AddRecordToDatabase());
                Thread.Sleep(5);
            }
        }
    }
}
