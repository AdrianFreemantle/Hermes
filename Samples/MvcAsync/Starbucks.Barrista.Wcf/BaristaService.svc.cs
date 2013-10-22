using System.ServiceModel;

using Hermes.Messaging.Wcf;

using Starbucks.Messages;

namespace Starbucks.Barrista.Wcf
{
    /// <summary>
    /// In order to expose a command as a WCF Service, you simply need to implement 
    /// CommandService with the generic parameter being the type you wish to handle.
    /// The command will be forwarded to the configured endpoint and the return code
    /// will be sent to the caller.
    /// </summary>    
    [ServiceBehavior(Name = "BaristaService")]
    public class BaristaService : CommandService<OrderCoffee>
    {
    }
}
