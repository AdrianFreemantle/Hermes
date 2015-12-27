using System;
using System.Threading.Tasks;
using Hermes.Serialization.Json;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace Hermes.Gateway.TestServer
{
    class Program
    {
        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            try
            {
                using (StartOwinHost())
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                }
               //await endpoint.Stop();

            }
            finally
            {
                //await endpoint.Stop();
            }
        }

        static IDisposable StartOwinHost()
        {
            string baseUrl = "http://localhost:12345/";
            
            StartOptions startOptions = new StartOptions(baseUrl)
            {
                ServerFactory = "Microsoft.Owin.Host.HttpListener",
            };

            return WebApp.Start(startOptions, builder =>
            {
                builder.UseCors(CorsOptions.AllowAll);
                MapToBus(builder);
            });
        }

        static void MapToBus(IAppBuilder builder)
        {
            OwinToBus owinToBus = new OwinToBus(new LocalBusStub(), new JsonObjectSerializer());
            builder.Map("/LocalBus", app => { app.Use(owinToBus.Middleware()); });
        }
    }
}
