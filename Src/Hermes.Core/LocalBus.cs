using System;
using Hermes.Ioc;
using Hermes.Logging;

namespace Hermes.Core
{
    //public class LocalBus : IInMemoryBus
    //{
    //    private static readonly ILog Logger = LogFactory.BuildLogger(typeof(LocalBus));

    //    private readonly IContainer container;
    //    private readonly IDispatchMessagesToHandlers messageDispatcher;

    //    public LocalBus(IDispatchMessagesToHandlers messageDispatcher, IContainer container)
    //    {
    //        Logger.Verbose("Using container {0}", container.GetHashCode());

    //        this.container = container;
    //        this.messageDispatcher = messageDispatcher;
    //    }

    //    public void Raise(object @event)
    //    {
    //        messageDispatcher.DispatchToHandlers(container, @event);
    //    }

    //    public void Execute(object command)
    //    {
    //        try
    //        {
    //            messageDispatcher.DispatchToHandlers(container, command);
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Error("Processing failed for message {0}: {1}", command, ex.Message);
    //            throw;
    //        }
    //    }
    //}
}