using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace Hermes.Messaging.Callbacks
{
    public class Callback : ICallback
    {
        static readonly Type AsyncControllerType;

        private readonly Guid messageId;

        static Callback()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies.Where(assembly => assembly.GetName().Name == "System.Web.Mvc"))
            {
                AsyncControllerType = assembly.GetType("System.Web.Mvc.AsyncController", false);
            }

            if (AsyncControllerType == null)
            {
                //We just initialize it to any type so we don't need to check for nulls.
                AsyncControllerType = typeof(BusAsyncResultEventArgs);
            }
        }

        /// <summary>
        /// Creates a new instance of the callback object storing the given message id.
        /// </summary>
        /// <param name="messageId"></param>
        public Callback(Guid messageId)
        {
            this.messageId = messageId;
        }

        /// <summary>
        /// Event raised when the Register method is called.
        /// </summary>
        public event EventHandler<BusAsyncResultEventArgs> Registered;

        /// <summary>
        /// Returns the message id this object was constructed with.
        /// </summary>
        public Guid MessageId
        {
            get { return messageId; }
        }

        Task<int> ICallback.Register()
        {
            return ((ICallback)this).Register(TimeSpan.MaxValue);
        }

        Task<int> ICallback.Register(TimeSpan timeout)
        {
            var asyncResult = ((ICallback)this).Register(null, null, timeout);
            var task = Task<int>.Factory.FromAsync(asyncResult, x =>
            {
                var cr = ((CompletionResult)x.AsyncState);

                return cr.ErrorCode;
            }, TaskCreationOptions.None, TaskScheduler.Default);

            return task;
        }               

        Task<T> ICallback.Register<T>()
        {
            return ((ICallback)this).Register<T>(TimeSpan.MaxValue); 
        }

        Task<T> ICallback.Register<T>(TimeSpan timeout)
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException(
                    "Register<T> can only be used with enumerations, use Register() to return an integer instead");

            var asyncResult = ((ICallback)this).Register(null, null, timeout);
            var task = Task<T>.Factory.FromAsync(asyncResult, x =>
            {
                var cr = ((CompletionResult)x.AsyncState);

                return (T)Enum.Parse(typeof(T), cr.ErrorCode.ToString(CultureInfo.InvariantCulture));
            }, TaskCreationOptions.None, TaskScheduler.Default);

            return task;
        }

        Task<T> ICallback.Register<T>(Func<CompletionResult, T> completion)
        {
            return ((ICallback)this).Register(completion, TimeSpan.MaxValue);
        }

        Task<T> ICallback.Register<T>(Func<CompletionResult, T> completion, TimeSpan timeout)
        {
            var asyncResult = ((ICallback)this).Register(null, null, timeout);

            var task = Task<T>.Factory.FromAsync(asyncResult, x => completion((CompletionResult)x.AsyncState),
                TaskCreationOptions.None, TaskScheduler.Default);

            return task;
        }

        Task ICallback.Register(Action<CompletionResult> completion)
        {
            var asyncResult = ((ICallback)this).Register(null, null);
            var task = Task.Factory.FromAsync(asyncResult, x => completion((CompletionResult)x.AsyncState),
                TaskCreationOptions.None, TaskScheduler.Default);

            return task;
        }

        IAsyncResult ICallback.Register(AsyncCallback callback, object state)
        {
            return ((ICallback)this).Register(callback, state, TimeSpan.MaxValue);
        }

        IAsyncResult ICallback.Register(AsyncCallback callback, object state, TimeSpan timeout)
        {
            var result = new BusAsyncResult(callback, state, timeout);

            if (Registered != null)
                Registered(this, new BusAsyncResultEventArgs { Result = result, MessageId = messageId });

            return result;    
        }

        void ICallback.Register<T>(Action<T> callback)
        {
            var page = callback.Target as Page;
            if (page != null)
            {
                (this as ICallback).Register(callback, page);
                return;
            }

            if (AsyncControllerType.IsInstanceOfType(callback.Target))
            {
                (this as ICallback).Register(callback, callback.Target);
                return;
            }

            var context = SynchronizationContext.Current;
            (this as ICallback).Register(callback, context);
        }

        void ICallback.Register<T>(Action<T> callback, object synchronizer)
        {
            if (!typeof(T).IsEnum && typeof(T) != typeof(int))
                throw new InvalidOperationException("Can only support registering callbacks for integer or enum types. The given type is: " + typeof(T).FullName);

            if (HttpContext.Current != null && synchronizer == null)
                throw new ArgumentNullException("synchronizer", "Hermes has detected that you're running in a web context but have passed in a null synchronizer. Please pass in a reference to a System.Web.UI.Page or a System.Web.Mvc.AsyncController.");

            if (synchronizer == null)
            {
                (this as ICallback).Register(GetCallbackInvocationActionFrom(callback), null);
                return;
            }

            if (synchronizer is Page)
            {
                (synchronizer as Page).RegisterAsyncTask(new PageAsyncTask(
                    (sender, e, cb, extraData) => (this as ICallback).Register(cb, extraData),
                    new EndEventHandler(GetCallbackInvocationActionFrom(callback)),
                    null,
                    null
                    ));
                return;
            }

            if (AsyncControllerType.IsInstanceOfType(synchronizer))
            {
                dynamic asyncController = synchronizer;
                asyncController.AsyncManager.OutstandingOperations.Increment();

                (this as ICallback).Register(GetMvcCallbackInvocationActionFrom(callback, asyncController.AsyncManager), null);

                return;
            }

            var synchronizationContext = synchronizer as SynchronizationContext;
            if (synchronizationContext != null)
            {
                (this as ICallback).Register(
                    ar => synchronizationContext.Post(
                        x => GetCallbackInvocationActionFrom(callback).Invoke(ar), null),
                    null
                    );
            }
        }

        static AsyncCallback GetMvcCallbackInvocationActionFrom<T>(Action<T> callback, dynamic am)
        {
            return asyncResult =>
            {
                HandleAsyncResult(callback, asyncResult);
                am.OutstandingOperations.Decrement();
            };
        }

        static AsyncCallback GetCallbackInvocationActionFrom<T>(Action<T> callback)
        {
            return asyncResult => HandleAsyncResult(callback, asyncResult);
        }

        static void HandleAsyncResult<T>(Action<T> callback, IAsyncResult asyncResult)
        {
            var cr = asyncResult.AsyncState as CompletionResult;
            if (cr == null) return;

            var action = callback as Action<int>;
            if (action != null)
            {
                action.Invoke(cr.ErrorCode);
            }
            else
            {
                callback((T)Enum.ToObject(typeof(T), cr.ErrorCode));
            }
        }
    }
}