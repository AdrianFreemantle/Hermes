using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Messaging;
using Hermes.Serialization;
using Microsoft.Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Hermes.Gateway
{
    public class OwinMiddlewareReceiver : IReceiveMessages
    {
        private readonly ISerializeObjects serializer;
        private Action<MessageContext> messageReceived;
        private CancellationToken cancellationToken;

        public OwinMiddlewareReceiver(ISerializeObjects serializer)
        {
            this.serializer = serializer;
        }

        public void Start(CancellationToken cancellationToken, Action<MessageContext> messageReceived)
        {
            this.messageReceived = messageReceived;
            this.cancellationToken = cancellationToken;
        }

        public Func<AppFunc, AppFunc> Middleware()
        {
            return _ => Invoke;
        }

        private async Task Invoke(IDictionary<string, object> env)
        {
            IOwinContext context = new OwinContext(env);
            IOwinResponse response = context.Response;

            if (cancellationToken.IsCancellationRequested)
            {
                BuildServiceUnavailableResponse(response);
                return;
            }

            object message = await GetMessage(context).ConfigureAwait(false);
            var messageContext = new MessageContext(message);

            await Task.Factory.StartNew(() => { messageReceived(messageContext); }, cancellationToken).ConfigureAwait(false);

            BuildOkayResponse(response, messageContext);
        }

        private static void BuildOkayResponse(IOwinResponse response, MessageContext messageContext)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ReasonPhrase = "OK";
            response.Write(messageContext.MessageId.ToString());
        }

        private static void BuildServiceUnavailableResponse(IOwinResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            response.ReasonPhrase = "Service Unavailable";
        }

        private async Task<object> GetMessage(IOwinContext context)
        {
            string typeName = context.Request.Headers.Get("MessageType");
            Type objectType = Type.GetType(typeName);
            string messageBody = await GetMessageBody(context).ConfigureAwait(false);
            
            return await DeserializeMessage(messageBody, objectType).ConfigureAwait(false);
        }

        private async Task<object> DeserializeMessage(string messageBody, Type objectType)
        {
            using (StringReader textReader = new StringReader(messageBody))
            {
                var serializedText = await textReader.ReadToEndAsync().ConfigureAwait(false);
                return serializer.DeserializeObject(serializedText, objectType);
            }
        }

        private async Task<string> GetMessageBody(IOwinContext context)
        {
            using (StreamReader streamReader = new StreamReader(context.Request.Body))
            {
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}

