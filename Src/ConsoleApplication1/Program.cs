using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hermes.Logging;
using Hermes.Monitoring.Statistics;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            LogFactory.BuildLogger = t => new ConsoleWindowLogger(t);
            var counter = new MessagesPerSeccondCounter();

            while (true)
            {
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
