using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Hermes.Messaging.Configuration;
using Starbucks.App_Start;

namespace Starbucks
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private static RequestorEndpoint endpoint;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ConfigureEndpoint();
        }

        protected void Application_End()
        {
            endpoint.Dispose();
        }

        private static void ConfigureEndpoint()
        {
            endpoint = new RequestorEndpoint();
            DependencyResolver.SetResolver(((MvcAutofacAdapter)Settings.RootContainer).BuildAutofacDependencyResolver());
            endpoint.Start();
        }
    }
}