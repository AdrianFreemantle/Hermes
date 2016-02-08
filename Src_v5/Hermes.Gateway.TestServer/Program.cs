using System;
using System.Security.Cryptography;
using System.Threading;
using Hermes.Serialization.Json;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace Hermes.Gateway.TestServer
{
    class Program
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static readonly OwinMessageReceiver LocalBusReciever = new OwinMessageReceiver(new JsonObjectSerializer());
        private static readonly OwinMessageReceiver AsyncBusReciever = new OwinMessageReceiver(new JsonObjectSerializer());

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
            LocalBusReciever.Start(CancellationTokenSource.Token, message =>
            {
                Console.WriteLine(@"{0}: Executing message {1} of type {2}", DateTime.Now.ToLongTimeString(), message.MessageId, message.GetType().FullName);                
            });

            AsyncBusReciever.Start(CancellationTokenSource.Token, message =>
            {
                Console.WriteLine(@"{0}: Sending message {1} of type {2}", DateTime.Now.ToLongTimeString(), message.MessageId, message.GetType().FullName);                
            });

            builder.Map("/LocalBus", app =>
            {
                app.Use<OwinSimpleMessageDeduplication>();
                app.Use(LocalBusReciever.Middleware());
            });

            builder.Map("/AsyncBus", app =>
            {
                app.Use(AsyncBusReciever.Middleware());
            });
        }
    }
}
