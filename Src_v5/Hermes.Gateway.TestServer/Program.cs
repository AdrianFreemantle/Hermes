using System;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Messaging;
using Hermes.Serialization.Json;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace Hermes.Gateway.TestServer
{
    class Program
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static readonly OwinMiddlewareReceiver OwinMiddlewareReceiver = new OwinMiddlewareReceiver(new JsonObjectSerializer());

        static void Main()
        {
            try
            {
                using (StartOwinHost())
                {
                    Console.WriteLine("Press any key to stop service");
                    Console.ReadKey();
                    CancellationTokenSource.Cancel();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server experienced a critical error: {0}", ex.Message);
            }
            finally
            {
                CancellationTokenSource.Cancel();
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
            Action<MessageContext> executeCommand = message =>
            {
                Console.WriteLine(@"Message {0} of type {1} recieved at {2}", message.MessageId, message.GetType().FullName, DateTime.Now.ToLongTimeString());                
            };

            OwinMiddlewareReceiver.Start(CancellationTokenSource.Token, executeCommand);

            builder.Map("/LocalBus", app =>
            {
                app.Use(OwinMiddlewareReceiver.Middleware());
            });
        }
    }
}
