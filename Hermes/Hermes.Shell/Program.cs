using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;

using Autofac;

using Hermes.Core;
using Hermes.Messages;
using Hermes.Messages.Attributes;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization;
using Hermes.Serialization.Json;
using Hermes.Transports;
using Hermes.Transports.SqlServer;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Shell
{
    class Program
    {
        private const string connection = @"Data Source=CHANDRA\SQLEXPRESS;Initial Catalog=AsbaBank;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        static void Main(string[] args)
        {
            Configuration.AddSetting(Configuration.ConnectionString, connection);

            var builder = new ContainerBuilder();
            var autofacServiceAdapter = new AutofacServiceAdapter(builder);

            builder.RegisterType<JsonObjectSerializer>().As<ISerializeObjects>().SingleInstance();
            builder.RegisterType<JsonMessageSerializer>().As<ISerializeMessages>().SingleInstance();
            builder.RegisterType<JsonMessageSerializer>().As<ISerializeMessages>().SingleInstance();
            builder.RegisterType<SqlMessageDequeueStrategy>().As<IMessageDequeueStrategy>().SingleInstance();
            builder.RegisterType<SqlServerMessageSender>().As<ISendMessages>().SingleInstance();
            builder.RegisterType<MessageProcessor>().As<IProcessMessages>().SingleInstance();
            
            builder.RegisterType<MessageHandlerFactory>().As<IBuildMessageHandlers>().InstancePerLifetimeScope();
            builder.RegisterType<MessageDispatcher>().As<IDispatchMessagesToHandlers>().InstancePerLifetimeScope();
            
            builder.RegisterType<MessageHandler>().As<IHandleMessage<SimpleMessage>>().As<IHandleMessage<SimpleMessage2>>().InstancePerLifetimeScope();

            builder.RegisterInstance(autofacServiceAdapter).As<IObjectBuilder>().As<IServiceLocator>();


            var queueCreator = new SqlServerQueueCreator();
            Address documentAddress = Address.Parse("Queues.Testing.Document");
            queueCreator.CreateQueueIfNecessary(documentAddress);

            var dequeue = autofacServiceAdapter.GetInstance<IMessageDequeueStrategy>();
            var messageProcessor = autofacServiceAdapter.GetInstance<IProcessMessages>();

            var receiver = new SqlServerMessageReceiver(dequeue, messageProcessor);
            receiver.Start(documentAddress);

            while (true)
            {
                var message1 = new SimpleMessage
                {
                    Hello = DateTime.Now.ToShortTimeString()
                };

                var message2 = new SimpleMessage2
                {
                    Hello = DateTime.Now.ToShortTimeString()
                };

                byte[] messages;

                using (var stream = new MemoryStream())
                {
                    autofacServiceAdapter.GetInstance<ISerializeMessages>();
                    var messageSerializer = autofacServiceAdapter.GetInstance<ISerializeMessages>();
                    messageSerializer.Serialize(new object[] { message1, message2 }, stream);
                    stream.Flush();
                    messages = stream.ToArray();
                }

                var sender = autofacServiceAdapter.GetInstance<ISendMessages>();

                sender.Send(new MessageEnvelope(Guid.NewGuid(), Guid.Empty, Address.Self, TimeSpan.MaxValue, true, new Dictionary<string, string>(), messages), documentAddress);
                System.Threading.Thread.Sleep(100);       
            }
        }
    }

    public class MessageHandler 
        : IHandleMessage<SimpleMessage>
        , IHandleMessage<SimpleMessage2>
    {
        public void Handle(SimpleMessage command)
        {
            Console.WriteLine("Handling SimpleMessage");
            SimulateTransientError();
        }

        public void Handle(SimpleMessage2 command)
        {
            Console.WriteLine("Handling SimpleMessage2");
            SimulateTransientError();
        }

        private static void SimulateTransientError()
        {
            //this code is here to simulate a transient error happening on the network
            if (DateTime.Now.Second % 2 == 0)
            {
                throw new Exception("Some random error has happened. This would normally mean our user must retry their action.");
            }
        }
    }

    [Retry(RetryCount = 3, RetryMilliseconds = 100)]
    public class SimpleMessage : IMessage
    {
        public string Hello { get; set; }
    }

    [Retry(RetryCount = 3, RetryMilliseconds = 100)]
    public class SimpleMessage2 : IMessage
    {
        public string Hello { get; set; }
    }
}
