using System;
using System.Reflection;
using System.Threading;

using Hermes;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.Ioc;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using MyDomain.Domain.Events;
using MyDomain.Infrastructure;
using MyDomain.Infrastructure.EntityFramework;
using MyDomain.Persistence.ReadModel;

namespace MyDomain.Projections
{
    class Program
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MyDomain;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        private static void Main(string[] args)
        {
            var configuration = Configure
                .Endpoint("Projections", new AutofacAdapter())
                //.UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .SecondLevelRetryPolicy(10, TimeSpan.FromSeconds(5))
                .ScanForHandlersIn(Assembly.Load(new AssemblyName("MyDomain.Persistence.ReadModel")))
                .SubscribeToEvent<ClaimEventIntimated>()
                .SubscribeToEvent<ClaimEventClosed>()
                .SubscribeToEvent<ClaimEventOpened>()
                .NumberOfWorkers(1);


            Settings.Builder.RegisterSingleton<IContextFactory>(new ContextFactory<MyDomainContext>("MyDomain"));
            Settings.Builder.RegisterType<EntityFrameworkUnitOfWork>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<UnitOfWorkManager>(DependencyLifecycle.InstancePerLifetimeScope);

            configuration.Start();

            var token = new CancellationTokenSource(TimeSpan.FromHours(10));

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(500);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }

    public class UnitOfWorkManager : IManageUnitOfWork
    {
        private readonly IUnitOfWork unitOfWork;

        public UnitOfWorkManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void Commit()
        {
            unitOfWork.Commit();
        }

        public void Rollback()
        {
            unitOfWork.Rollback();
        }
    }
}
