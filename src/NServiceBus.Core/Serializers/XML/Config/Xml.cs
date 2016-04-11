﻿namespace NServiceBus
{
    using System;
    using Features;
    using Serialization;

    /// <summary>
    /// Defines the capabilities of the XML serializer
    /// </summary>
    public class XmlSerializer : SerializationDefinition
    {
        internal override Type ProvidedByFeature()
        {
            return typeof(XmlSerialization);
        }
    }
}