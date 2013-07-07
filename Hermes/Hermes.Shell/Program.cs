using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;

using Autofac;

using Hermes.Configuration;
using Hermes.Core;
using Hermes.Messages;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization;
using Hermes.Serialization.Json;
using Hermes.Transports;
using Hermes.Transports.SqlServer;

namespace Hermes.Shell
{
    class Program
    {
        private const string connection = @"Data Source=CHANDRA\SQLEXPRESS;Initial Catalog=AsbaBank;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";
        static Address testQueue;

        static void Main(string[] args)
        {
            Initialize();

            var messageSerializer = Settings.Builder.GetInstance<ISerializeMessages>();

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
                    messageSerializer.Serialize(new object[] { message1, message2 }, stream);
                    stream.Flush();
                    messages = stream.ToArray();
                }

                var sender = Settings.Builder.GetInstance<ISendMessages>();

                sender.Send(new MessageEnvelope(Guid.NewGuid(), Guid.Empty, Address.Self, TimeSpan.MaxValue, true, new Dictionary<string, string>(), messages), testQueue);
                System.Threading.Thread.Sleep(500);       
            }
        }

        private static void Initialize()
        {
            Configure.With()
                     .ConnectionString(connection)
                     .ObjectBuilder(ConfigureBuilder())
                     .ConsoleWindowLogger();

            InitializeQueues();
            InitializeReceiver();
        }
        
        private static void InitializeReceiver()
        {
            var dequeue = Settings.Builder.GetInstance<IMessageDequeueStrategy>();
            var messageProcessor = Settings.Builder.GetInstance<IProcessMessages>();
            var receiver = new SqlServerMessageReceiver(dequeue, messageProcessor);
            receiver.Start(testQueue);
        }

        private static void InitializeQueues()
        {
            var queueCreator = new SqlServerQueueCreator();
            testQueue = Address.Parse("Queues.Testing.MyTestQueue");
            queueCreator.CreateQueueIfNecessary(testQueue);
        }

        private static AutofacServiceAdapter ConfigureBuilder()
        {
            var builder = new ContainerBuilder();
            var autofacServiceAdapter = new AutofacServiceAdapter();
            builder.RegisterType<JsonObjectSerializer>().As<ISerializeObjects>().SingleInstance();
            builder.RegisterType<JsonMessageSerializer>().As<ISerializeMessages>().SingleInstance();
            builder.RegisterType<JsonMessageSerializer>().As<ISerializeMessages>().SingleInstance();
            builder.RegisterType<SqlMessageDequeueStrategy>().As<IMessageDequeueStrategy>().SingleInstance();
            builder.RegisterType<SqlServerMessageSender>().As<ISendMessages>().SingleInstance();
            builder.RegisterType<MessageProcessor>().As<IProcessMessages>().SingleInstance();
            builder.RegisterType<MessageHandlerFactory>().As<IBuildMessageHandlers>().InstancePerLifetimeScope();
            builder.RegisterType<MessageDispatcher>().As<IDispatchMessagesToHandlers>().InstancePerLifetimeScope();
           
            builder.RegisterType<MessageHandler>()
                   .As<IHandleMessage<SimpleMessage>>()
                   .As<IHandleMessage<SimpleMessage2>>()
                   .InstancePerLifetimeScope();

            builder.RegisterInstance(autofacServiceAdapter).As<IObjectBuilder>();

            autofacServiceAdapter.lifetimeScope = builder.Build();
            return autofacServiceAdapter;
        }
    }
}
