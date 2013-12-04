using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Hermes.Messaging;
using Hermes.Messaging.Configuration;

using IntegrationTest.Contracts;

namespace IntegrationTest.Client
{
    class Program
    {
        const int NumberOfMessageToSend = 100000; //one hundered thousand

        static void Main(string[] args)
        {
            var endpoint =  new RequestorEndpoint();
            endpoint.Start();
            var bus = Settings.RootContainer.GetInstance<IMessageBus>();

            for (int i = 0; i < NumberOfMessageToSend; i++)
            {
                bus.Send(new AddRecordToDatabase());
                Thread.Sleep(5);
            }
        }
    }
}
