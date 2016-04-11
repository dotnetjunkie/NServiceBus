﻿namespace NServiceBus
{
    using System;
    using System.Transactions;
    using NServiceBus.DataBus;
    using Pipeline;
    using Pipeline.Contexts;

    class DataBusReceiveBehavior : IBehavior<IncomingContext>
    {
        public IDataBus DataBus { get; set; }

        public IDataBusSerializer DataBusSerializer { get; set; }

        public Conventions Conventions { get; set; }

        public void Invoke(IncomingContext context, Action next)
        {
            var message = context.IncomingLogicalMessage.Instance;

            foreach (var property in Conventions.GetDataBusProperties(message))
            {
                var propertyValue = property.GetValue(message, null);

                var dataBusProperty = propertyValue as IDataBusProperty;
                string headerKey;

                if (dataBusProperty != null)
                {
                    headerKey = dataBusProperty.Key;
                }
                else
                {
                    headerKey = String.Format("{0}.{1}", message.GetType().FullName, property.Name);
                }

                string dataBusKey;

                if (!context.IncomingLogicalMessage.Headers.TryGetValue("NServiceBus.DataBus." + headerKey, out dataBusKey))
                {
                    continue;
                }

                using (new TransactionScope(TransactionScopeOption.Suppress))
                using (var stream = DataBus.Get(dataBusKey))
                {
                    var value = DataBusSerializer.Deserialize(stream);

                    if (dataBusProperty != null)
                    {
                        dataBusProperty.SetValue(value);
                    }
                    else
                    {
                        property.SetValue(message, value, null);
                    }
                }
            }

            next();
        }

        public class Registration : RegisterStep
        {
            public Registration() : base("DataBusReceive", typeof(DataBusReceiveBehavior), "Copies the databus shared data back to the logical message")
            {
                InsertAfter(WellKnownStep.MutateIncomingMessages);
                InsertBefore(WellKnownStep.InvokeHandlers);
            }
        }
    }
}