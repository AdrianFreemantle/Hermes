using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Messaging;
using Hermes.Serialization;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Hermes.Gateway
{
    public class OwinToBus
    {
        private readonly ISerializeObjects serializer;
        private readonly ILocalBus bus;

        public OwinToBus(ILocalBus bus, ISerializeObjects serializer)
        {
            this.bus = bus;
            this.serializer = serializer;
        }

        public Func<AppFunc, AppFunc> Middleware()
        {
            return _ => Invoke;
        }

        private async Task Invoke(IDictionary<string, object> environment)
        {
            IDictionary<string, string[]> requestHeaders = (IDictionary<string, string[]>)environment["owin.RequestHeaders"];
            string typeName = requestHeaders["MessageType"].Single();
            Type objectType = Type.GetType(typeName);

            string messageBody = await GetMessageBody(environment).ConfigureAwait(false);
            IDomainCommand command = await Deserialize(messageBody, objectType);
            
            await Task.Factory.StartNew(() => bus.Execute(command)).ConfigureAwait(false);
        }

        private async Task<IDomainCommand> Deserialize(string messageBody, Type objectType)
        {
            using (StringReader textReader = new StringReader(messageBody))
            {
                var serializedText = await textReader.ReadToEndAsync().ConfigureAwait(false);
                return (IDomainCommand)serializer.DeserializeObject(serializedText, objectType);
            }
        }

        private async Task<string> GetMessageBody(IDictionary<string, object> environment)
        {
            using (Stream requestStream = (Stream)environment["owin.RequestBody"])
            using (StreamReader streamReader = new StreamReader(requestStream))
            {
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}

