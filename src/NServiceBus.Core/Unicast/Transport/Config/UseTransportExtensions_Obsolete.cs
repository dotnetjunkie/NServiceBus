#pragma warning disable 1591
// ReSharper disable UnusedParameter.Global
namespace NServiceBus
{
    using System;
    using Transports;

    public static partial class UseTransportExtensions
    {
        [ObsoleteEx(
            Message = "Use `configuration.UseTransport<T>()`, where `configuration` is an instance of type `BusConfiguration`.",
            RemoveInVersion = "6.0",
            TreatAsErrorFromVersion = "5.0")]
        public static Configure UseTransport<T>(this Configure config, Action<TransportConfiguration> customizations = null) where T : TransportDefinition
        {
            throw new InvalidOperationException();
        }

        [ObsoleteEx(
            Message = "Use `configuration.UseTransport(transportDefinitionType)`, where `configuration` is an instance of type `BusConfiguration`.", 
            RemoveInVersion = "6.0", 
            TreatAsErrorFromVersion = "5.0")]
        public static Configure UseTransport(this Configure config, Type transportDefinitionType, Action<TransportConfiguration> customizations = null)
        {
            throw new InvalidOperationException();
        }
    }
}