using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Hermes.Gateway
{
    public class OwinSimpleMessageDeduplication : OwinMiddleware
    {
        private const int QueueCapacity = 100;
        private static readonly object SyncLock = new object();
        private static readonly Queue<Guid> MessageHashQueue = new Queue<Guid>(QueueCapacity);
        
        public OwinSimpleMessageDeduplication(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            string messageBody = await GetMessageBody(context).ConfigureAwait(false);

            Guid messageHash = DeterministicGuid.Create(messageBody);
            bool isDuplicate = true;

            lock (SyncLock)
            {
                if (!MessageHashQueue.Contains(messageHash))
                {
                    if (MessageHashQueue.Count >= QueueCapacity)
                        MessageHashQueue.Dequeue();

                    MessageHashQueue.Enqueue(messageHash);
                    isDuplicate = false;
                }
            }

            if (isDuplicate)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                context.Response.ReasonPhrase = "Duplicate Request";
            }
            else
            {
                await Next.Invoke(context);
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