using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Hermes.Messaging.Management.App_Start;
using Hermes.Messaging.Management.Services;
using ServiceStack.Razor;
using ServiceStack.WebHost.Endpoints;

namespace Hermes.Messaging.Management
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public class HelloAppHost : AppHostBase
        {
            //Tell Service Stack the name of your application and where to find your web services
            public HelloAppHost() : base("Hello Web Services", typeof(TableSchemaService).Assembly) { }

            public override void Configure(Funq.Container container)
            {
                SetConfig(new EndpointHostConfig {ServiceStackHandlerFactoryPath = "api"});
                Plugins.Add(new RazorFormat());
            }
        }

        protected void Application_Start()
        {
            new HelloAppHost().Init();
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}