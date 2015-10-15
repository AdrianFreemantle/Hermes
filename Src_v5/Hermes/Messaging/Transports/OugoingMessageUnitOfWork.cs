using System;
using System.Collections.Generic;
using Hermes.Persistence;
using Hermes.Pipes;

namespace Hermes.Messaging.Transports
{
    public class OugoingMessageUnitOfWork : IUnitOfWork
    {
        private readonly ModulePipeFactory<MessageContext> outgoingPipelineFactory;
        private readonly List<MessageContext> pendingMessages;
        private bool disposed;

        public OugoingMessageUnitOfWork(ModulePipeFactory<MessageContext> outgoingPipelineFactory)
        {
            this.outgoingPipelineFactory = outgoingPipelineFactory;
            pendingMessages = new List<MessageContext>();
        }

        public void Enqueue(MessageContext outgoingMessage)
        {
            pendingMessages.Add(outgoingMessage);
        }

        public void Commit()
        {
            foreach (var pendingMessage in pendingMessages)
            {
                var pipeline = outgoingPipelineFactory.Build();
                pipeline.Invoke(pendingMessage);
            }
        }

        public void Rollback()
        {
            pendingMessages.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                pendingMessages.Clear();
        
            disposed = true;
        }
    }
}