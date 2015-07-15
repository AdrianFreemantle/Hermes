using System;
using System.Diagnostics;
using System.Messaging;
using System.Threading;
using System.Transactions;
using Hermes.Failover;
using Hermes.Logging;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Transports.Msmq
{
    public class MsmqReceiver : IReceiveMessages, IDisposable
    {
        public bool PurgeOnStartup { get; set; }
        public MsmqUnitOfWork UnitOfWork { get; set; }

        private Action<TransportMessage> processMessage;

        public MsmqReceiver()
        {
            if (!Address.Local.Machine.Equals(RuntimeEnvironment.MachineName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(String.Format("Input queue [{0}] must be on the same machine as this process [{1}].", Address.Local, RuntimeEnvironment.MachineName));
            }

            queue = new MessageQueue(MsmqUtilities.GetFullPath(Address.Local), false, true, QueueAccessMode.Receive);

            var messageReadPropertyFilter = new MessagePropertyFilter
            {
                Body = true,
                TimeToBeReceived = true,
                Recoverable = true,
                Id = true,
                ResponseQueue = true,
                CorrelationId = true,
                Extension = true,
                AppSpecific = true
            };

            queue.MessageReadPropertyFilter = messageReadPropertyFilter;

            if (PurgeOnStartup)
            {
                queue.Purge();
            }
        }

        public void Start(Action<TransportMessage> handleMessage)
        {
            processMessage = handleMessage;

            MessageQueue.ClearConnectionCache();
            maximumConcurrencyLevel = Settings.NumberOfWorkers; ;
            throttlingSemaphore = new SemaphoreSlim(maximumConcurrencyLevel, maximumConcurrencyLevel);

            queue.PeekCompleted += OnPeekCompleted;

            CallPeekWithExceptionHandling(() => queue.BeginPeek());
        }
        
        public void Stop()
        {
            queue.PeekCompleted -= OnPeekCompleted;

            stopResetEvent.WaitOne();
            DrainStopSemaphore();
            queue.Dispose();
        }

        void DrainStopSemaphore()
        {
            Logger.Debug("Drain stopping 'Throttling Semaphore'.");
            for (var index = 0; index < maximumConcurrencyLevel; index++)
            {
                Logger.Debug(string.Format("Claiming Semaphore thread {0}/{1}.", index + 1, maximumConcurrencyLevel));
                throttlingSemaphore.Wait();
            }
            Logger.Debug("Releasing all claimed Semaphore threads.");
            throttlingSemaphore.Release(maximumConcurrencyLevel);

            throttlingSemaphore.Dispose();
        }

        public void Dispose()
        {
            // Injected
        }

        void OnPeekCompleted(object sender, PeekCompletedEventArgs peekCompletedEventArgs)
        {
            stopResetEvent.Reset();

            CallPeekWithExceptionHandling(() => queue.EndPeek(peekCompletedEventArgs.AsyncResult));

            throttlingSemaphore.Wait();

            WorkerTaskFactory.Start(Action, CancellationToken.None);
         
            //We using an AutoResetEvent here to make sure we do not call another BeginPeek before the Receive has been called
            peekResetEvent.WaitOne();

            CallPeekWithExceptionHandling(() => queue.BeginPeek());

            stopResetEvent.Set();
        }

        void Action(object obj)
        {
            try
            {
                Message message;

                TransportMessage transportMessage = null;
                if (Settings.DisableDistributedTransactions)
                {
                    using (var msmqTransaction = new MessageQueueTransaction())
                    {
                        msmqTransaction.Begin();
                        message = ReceiveMessage(() => queue.Receive(receiveTimeout, msmqTransaction));

                        if (message == null)
                        {
                            msmqTransaction.Commit();
                            return;
                        }

                        try
                        {
                            UnitOfWork.SetTransaction(msmqTransaction);
                            transportMessage = MsmqUtilities.Convert(message);
                            processMessage(transportMessage);
                            msmqTransaction.Commit();
                        }
                        catch 
                        {
                            msmqTransaction.Abort();
                            throw;
                        }
                        finally
                        {
                            UnitOfWork.ClearTransaction();
                        }
                    }
                }
                else
                {
                    using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.Required))
                    {
                        message = ReceiveMessage(() => queue.Receive(receiveTimeout, MessageQueueTransactionType.Automatic));

                        if (message != null)
                        {
                            transportMessage = MsmqUtilities.Convert(message);
                            processMessage(transportMessage);
                        }
                        
                        scope.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                SytemCircuitBreaker.Trigger(ex);
            }
            finally
            {
                throttlingSemaphore.Release();
            }
        }

        void CallPeekWithExceptionHandling(Action action)
        {
            try
            {
                action();
            }
            catch (MessageQueueException messageQueueException)
            {
                CriticalError.Raise("", messageQueueException);
            }
        }

        [DebuggerNonUserCode]
        Message ReceiveMessage(Func<Message> receive)
        {
            Message message = null;
            try
            {
                message = receive();
            }
            catch (MessageQueueException messageQueueException)
            {
                if (messageQueueException.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                {
                    //We should only get an IOTimeout exception here if another process removed the message between us peeking and now.
                    return null;
                }

                CriticalError.Raise("", messageQueueException);
            }
            catch (Exception ex)
            {
                SytemCircuitBreaker.Trigger(ex, "Error in receiving messages.");
            }
            finally
            {
                peekResetEvent.Set();
            }
            return message;
        }

        static readonly ILog Logger = LogFactory.Build<MsmqReceiver>();
        readonly AutoResetEvent peekResetEvent = new AutoResetEvent(false);
        readonly TimeSpan receiveTimeout = TimeSpan.FromSeconds(1);
        readonly ManualResetEvent stopResetEvent = new ManualResetEvent(true);
        SemaphoreSlim throttlingSemaphore;
        int maximumConcurrencyLevel;

        readonly MessageQueue queue;
    }
}