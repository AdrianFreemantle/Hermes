using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hermes.Messaging.ProcessManagement;

using Starbucks.Messages;

namespace Starbucks.Cashier
{
    public class OrderState : IContainProcessManagerData
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public Guid OriginalMessageId { get; set; }
        public Coffee Coffee { get; set; }
    }


    public class OrderManager : ProcessManager<OrderState>
    {
        
    }
}
