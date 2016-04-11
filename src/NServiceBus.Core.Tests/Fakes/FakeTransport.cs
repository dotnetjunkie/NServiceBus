﻿namespace NServiceBus.Core.Tests.Fakes
{
    using System;
    using Unicast.Transport;

    public class FakeTransport : ITransport
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
        }

        public bool IsStarted { get; set; }
        public Address InputAddress { get; set; }
        public void Start(Address localAddress)
        {
            IsStarted = true;
            InputAddress = localAddress;
        }

        public int HasChangedMaximumConcurrencyLevelNTimes { get; set; }

        public int MaximumConcurrencyLevel { get; private set; }

        public void ChangeMaximumConcurrencyLevel(int maximumConcurrencyLevel)
        {
            MaximumConcurrencyLevel = maximumConcurrencyLevel;
            HasChangedMaximumConcurrencyLevelNTimes++;
        }

        public void AbortHandlingCurrentMessage()
        {
            throw new NotImplementedException();
        }

        public int MaximumMessageThroughputPerSecond { get; private set; }

        public bool IsEventAssigned
        {
            get { return TransportMessageReceived != null; }
        }

        public void RaiseEvent(TransportMessage message)
        {
            TransportMessageReceived(this, new TransportMessageReceivedEventArgs(message));
        }

        public void ChangeMaximumMessageThroughputPerSecond(int maximumMessageThroughputPerSecond)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        public event EventHandler<StartedMessageProcessingEventArgs> StartedMessageProcessing;
        public event EventHandler<FinishedMessageProcessingEventArgs> FinishedMessageProcessing;
        public event EventHandler<FailedMessageProcessingEventArgs> FailedMessageProcessing;
    }
}