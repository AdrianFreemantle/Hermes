using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using Starbucks.Messages;

namespace Starbucks
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static IMessageBus Bus { get; private set; }

        private const string ConnectionString = @"Data Source=CG-T-SQL-03V;Initial Catalog=CG_T_DB_MSGBRKR;User ID=CG_T_USR_SYNAFreemantle;Password=vimes Sep01";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ConfigureHermes();
            Bus = Settings.MessageBus;
        }

        private void ConfigureHermes()
        {
            Configure
                .ClientEndpoint("Starbucks", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .UseDistributedTransaction()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .RegisterMessageRoute<BuyCoffee>(Address.Parse("Barrista"))
                .ScanForHandlersIn(Assembly.GetExecutingAssembly())
                .Start();
        }
    }
}