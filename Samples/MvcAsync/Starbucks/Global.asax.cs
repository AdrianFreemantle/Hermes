using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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
            
            endpoint = new RequestorEndpoint();
            endpoint.Start();

            ConfigureMvcAutofac();
        }

        protected void Application_End()
        {
            endpoint.Dispose();
        }

        private static void ConfigureMvcAutofac()
        {
            var autofacAdapter = new MvcAutofacAdapter();
            autofacAdapter.RegisterSingleton(endpoint.MessageBus);
            autofacAdapter.BuildContainer();
            DependencyResolver.SetResolver(autofacAdapter.BuildAutofacDependencyResolver());
        }
    }
}