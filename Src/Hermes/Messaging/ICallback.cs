using System;
using System.Threading.Tasks;

using Hermes.Messaging.Callbacks;

namespace Hermes.Messaging
{
    public interface ICallback
    {
        /// <summary>
        /// Registers a callback to be invoked when a response arrives to the message sent.
        /// The return code is returned as an int
        /// </summary>
        Task<int> Register();

        Task<int> Register(TimeSpan timeout);

        /// <summary>
        /// Registers a callback to be invoked when a response arrives to the message sent.
        /// The return code is cast to the given enumerated type - T.
        /// </summary>
        /// <typeparam name="T">An enumeration type or an integer.</typeparam>
        Task<T> Register<T>();

        Task<T> Register<T>(TimeSpan timeout);

        /// <summary>
        /// Registers a function to be invoked when a response arrives to the message sent.
        /// Returns a Task that can be used with async/await operations.
        /// </summary>
        /// <param name="completion">A function to call upon completion that returns a value of type T.</param>
        /// <typeparam name="T">The type of the value to be returned from the function.</typeparam>
        /// <returns>A Task that can be used with async/await operations.</returns>
        Task<T> Register<T>(Func<CompletionResult, T> completion);

        Task<T> Register<T>(Func<CompletionResult, T> completion, TimeSpan timeout);

        /// <summary>
        /// Registers an action to be invoked when a response arrives to the message sent.
        /// Returns a Task that can be used with async/await operations.
        /// </summary>
        /// <param name="completion">An action to call upon completion that does not return a value.</param>
        /// <returns>A Task that can be used with async/await operations.</returns>
        Task Register(Action<CompletionResult> completion);

        /// <summary>
        /// Registers a callback to be invoked when a response arrives to the message sent.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="state">State that will be passed to the callback method.</param>
        /// <returns>An IAsyncResult useful for integration with ASP.NET async tasks.</returns>
        IAsyncResult Register(AsyncCallback callback, object state);

        IAsyncResult Register(AsyncCallback callback, object state, TimeSpan timeout);

        /// <summary>
        /// Registers a callback to be invoked when a response arrives to the message sent.
        /// The return code is cast to the given enumerated type - T.
        /// </summary>
        /// <typeparam name="T">An enumeration type or an integer.</typeparam>
        /// <param name="callback"></param>
        void Register<T>(Action<T> callback);

        /// <summary>
        /// Registers a callback to be invoked when a response arrives to the message sent.
        /// The return code is cast to the given enumerated type - T.
        /// Pass either a System.Web.UI.Page or a System.Web.Mvc.AsyncController as the synchronizer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        /// <param name="synchronizer"></param>
        void Register<T>(Action<T> callback, object synchronizer);
    }
}