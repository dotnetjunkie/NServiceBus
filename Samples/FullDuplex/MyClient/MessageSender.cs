﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using MyMessages;
using NServiceBus;

namespace MyClient
{
    public class MessageSender
    {
        private readonly int messagesToSend;

        public MessageSender(int count, IBus bus)
        {
            messagesToSend = count;
            Bus = bus;
        }

        public IBus Bus { get; set; }

        public void SendMessages()
        {
            int counter = 0;
            var requestDataMessage = new RequestDataMessage[messagesToSend];

            while (counter < messagesToSend)
            {
                requestDataMessage[counter] = new RequestDataMessage
                                                  {
                                                      DataId = new Guid(),
                                                      String = counter.ToString(CultureInfo.InvariantCulture)
                                                  };
                counter++;
            }
            counter = 0;
            var watch = new Stopwatch();
            watch.Start();
            while (counter < messagesToSend)
            {
                Bus.Send(requestDataMessage[counter]);
                counter++;
            }
            watch.Stop();
            Console.WriteLine("ThreadId: [{0}]. Time to send: [{1}] messages is: [{2}]",
                              Thread.CurrentThread.ManagedThreadId, counter, watch.Elapsed.TotalSeconds);
        }
    }
}