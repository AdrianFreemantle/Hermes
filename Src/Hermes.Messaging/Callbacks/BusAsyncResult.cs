using System;
using System.Threading;

using Hermes.Logging;

using Timer = System.Timers.Timer;

namespace Hermes.Messaging.Callbacks
{
    public delegate void BusAsyncResultTimeoutHandler(object sender, EventArgs e);

    public class BusAsyncResult : IAsyncResult
    {               
        private readonly static ILog log = LogFactory.BuildLogger(typeof(BusAsyncResult));

        private readonly AsyncCallback callback;
        private readonly CompletionResult result;
        private volatile bool completed;
        private readonly ManualResetEvent sync;
        private Timer timeoutTimer;
        private bool hasTimedOut;

        public event BusAsyncResultTimeoutHandler OnTimeout;

        [Obsolete]
        public BusAsyncResult(AsyncCallback callback, object state)
            : this(callback, state, TimeSpan.MaxValue)
        {
        }

        /// <summary>
        /// Creates a new object storing the given callback and state.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public BusAsyncResult(AsyncCallback callback, object state, TimeSpan timeout)
        {
            this.callback = callback;
            result = new CompletionResult {State = state};

            sync = new ManualResetEvent(false);

            if (timeout != TimeSpan.MaxValue)
            {
                timeoutTimer = new Timer(timeout.TotalMilliseconds);
                timeoutTimer.AutoReset = false;
                timeoutTimer.Elapsed += timeoutTimer_Elapsed;
                timeoutTimer.Start();
            }
        }

        void timeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (sync)
            {
                if (completed)
                    return;

                hasTimedOut = true;
                result.ErrorCode = -1;
                result.Messages = new object[0];
                TriggerTimeoutEvent();
                CompleteCallback();
            }
        }

        private void TriggerTimeoutEvent()
        {
            if (OnTimeout == null)
                return;

            try
            {
                OnTimeout(this, new EventArgs());
            }
            catch (Exception e)
            {
                OnTimeout = null;
                log.Error(callback.ToString(), e);
            }
        }

        /// <summary>
        /// Stores the given error code and messages, 
        /// releases any blocked threads,
        /// and invokes the previously given callback.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="messages"></param>
        public void Complete(int errorCode, object[] messages)
        {
            lock (sync)
            {
                if (hasTimedOut)
                    return;

                result.ErrorCode = errorCode;
                result.Messages = messages;
                completed = true;
                CompleteCallback();
            }
        }

        private void CompleteCallback()
        {
            if (callback != null)
            {
                try
                {
                    callback(this);
                }
                catch (Exception e)
                {
                    log.Error(callback.ToString(), e);
                }
            }

            sync.Set();
        }

        #region IAsyncResult Members

        /// <summary>
        /// Returns a completion result containing the error code, messages, and state.
        /// </summary>
        public object AsyncState
        {
            get
            {
                if (hasTimedOut)
                {
                    throw new TimeoutException("The server took too long to respond.");
                }

                return this.result;
            }
        }

        /// <summary>
        /// Returns a handle suitable for blocking threads.
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get { return this.sync; }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        public bool CompletedSynchronously
        {
            get { return false; }
        }

        /// <summary>
        /// Returns if the operation has completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return this.completed; }
        }

        #endregion
    }
}
