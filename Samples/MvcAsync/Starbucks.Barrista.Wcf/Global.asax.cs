using System;
using System.Web;

namespace Starbucks.Barrista.Wcf
{
    public class Global : HttpApplication
    {
        RequestorEndpoint endpoint;

        protected void Application_Start(object sender, EventArgs e)
        {
            endpoint = new RequestorEndpoint();
            endpoint.Start();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            endpoint.Dispose();
        }
    }
}