﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

using Hermes.Backoff;
using Hermes.Configuration;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Transports;

namespace Hermes.Core
{  
    public class Receiver : IReceiveMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Receiver));

        private CancellationTokenSource tokenSource;
        private readonly IMessageDequeueStrategy dequeueStrategy;
        private readonly IProcessMessages messageProcessor;

        public Receiver(IMessageDequeueStrategy dequeueStrategy, IProcessMessages messageProcessor)
        {
            this.dequeueStrategy = dequeueStrategy;
            this.messageProcessor = messageProcessor;
        }

        public void Start()
        {
            tokenSource = new CancellationTokenSource();

            for (int i = 0; i < Settings.NumberOfWorkers; i++)
            {
                StartThread();
            }
        }

        private void StartThread()
        {
            CancellationToken token = tokenSource.Token;
            Task.Factory.StartNew(WorkerAction, token, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }

        public void WorkerAction(object obj)
        {
            var backoff = new BackOff(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(1000));
            var cancellationToken = (CancellationToken)obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                DequeueNextMessage(backoff);
            }
        }

        private void DequeueNextMessage(BackOff backoff)
        {
            bool foundWork = false;

            try
            {
                foundWork = DequeueWork();
            }
            catch (Exception ex)
            {
                Logger.Fatal("Error while attempting to dequeue work: {0}", ex.GetFullExceptionMessage());
            }
            finally
            {
                SlowDownPollingIfNoWorkAvailable(foundWork, backoff);
            }
        }

        private bool DequeueWork()
        {
            using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.Required))
            {
                try
                {
                    return TryDequeueWork();
                }
                finally
                {
                    scope.Complete();
                }
            }
        }

        private bool TryDequeueWork()
        {
            TransportMessage transportMessage = dequeueStrategy.Dequeue(Address.Local);

            if (transportMessage == TransportMessage.Undefined)
            {
                return false;
            }
            messageProcessor.ProcessEnvelope(transportMessage);
            return true;
        }

        private static void SlowDownPollingIfNoWorkAvailable(bool foundWork, BackOff backoff)
        {
            if (foundWork)
            {
                backoff.Reset();
            }
            else
            {
                backoff.Delay();
            }
        }        
    }
}
