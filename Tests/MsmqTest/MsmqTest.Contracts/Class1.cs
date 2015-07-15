using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmqTest.Contracts
{
    public interface ICommand
    {
    }

    public interface IEvent
    {
    }


    public class PrintNumber : ICommand
    {
        public long Number { get; set; }

        public PrintNumber() { }

        public PrintNumber(long number)
        {
            Number = number;
        }
    }
}
