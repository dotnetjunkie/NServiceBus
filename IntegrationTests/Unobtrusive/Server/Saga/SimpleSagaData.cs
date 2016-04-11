using System;
using NServiceBus.Saga;

namespace Server.Saga
{
    public class SimpleSagaData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

        [Unique]
        public Guid OrderId { get; set; }
    }
}