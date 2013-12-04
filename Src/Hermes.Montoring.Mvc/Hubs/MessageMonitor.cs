using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.AspNet.SignalR;

namespace Hermes.Montoring.Mvc.Hubs
{
    public class MessageMonitor : Hub
    {
        private static readonly Timer timer = new Timer();
        private static readonly Random random = new Random();

        private static readonly Queue<int> goodQueue = new Queue<int>();
        private static readonly Queue<int> badQueue = new Queue<int>();

        public MessageMonitor()
        {
            timer.Interval = 500;
            timer.AutoReset = true;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            goodQueue.Enqueue(random.Next(50));
            badQueue.Enqueue(random.Next(50));

            if (goodQueue.Count > 100)
            {
                goodQueue.Dequeue();
            }

            if (badQueue.Count > 100)
            {
                badQueue.Dequeue();
            }

            Clients.All.addMessageResultToPage(goodQueue.ToArray(), badQueue.ToArray());
        }
    }
}