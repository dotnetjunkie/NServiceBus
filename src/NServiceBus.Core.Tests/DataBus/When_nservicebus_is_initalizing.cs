namespace NServiceBus.Core.Tests.DataBus
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using NServiceBus.DataBus;
    using NServiceBus.DataBus.InMemory;
    using NServiceBus.Features;
    using NUnit.Framework;

    [TestFixture]
    public class When_nservicebus_is_initializing
    {
        [Test]
        public void Databus_should_be_activated_if_a_databus_property_is_found()
        {
            var builder = new BusConfiguration();

            builder.EndpointName("xyz");
            builder.TypesToScan(new[]{typeof(MessageWithDataBusProperty)});
            builder.RegisterComponents(c => c.ConfigureComponent<InMemoryDataBus>(DependencyLifecycle.SingleInstance));
            
            var config = builder.BuildConfiguration();

            Assert.True(new DataBus().CheckPrerequisites(new FeatureConfigurationContext(config)).IsSatisfied);
        }

        [Test]
        public void Databus_should_not_be_activated_if_no_databus_property_is_found()
        {
            var builder = new BusConfiguration();

            builder.EndpointName("xyz");
            builder.TypesToScan(new[] { typeof(MessageWithoutDataBusProperty) });

            var feature = new DataBus();

            Assert.False(feature.CheckPrerequisites(new FeatureConfigurationContext(builder.BuildConfiguration())).IsSatisfied);
        }

        [Test]
        public void Should_throw_if_propertyType_is_not_serializable()
        {
            if (!Debugger.IsAttached)
            {
                Assert.Ignore("This only work in debug mode.");
            }

            var builder = new BusConfiguration();
            builder.EndpointName("xyz");
            builder.TypesToScan(new[]
                {
                    typeof(MessageWithNonSerializableDataBusProperty)
                });
            builder.Conventions().DefiningDataBusPropertiesAs(p => p.Name.EndsWith("DataBus"));
            
            var feature = new DataBus();

            Assert.Throws<InvalidOperationException>(() => feature.CheckPrerequisites(new FeatureConfigurationContext(builder.BuildConfiguration())));
        }

        [Test]
        public void Should_not_throw_propertyType_is_not_serializable_if_a_IDataBusSerializer_is_already_registered()
        {
            if (!Debugger.IsAttached)
            {
                Assert.Ignore("This only work in debug mode.");
            }

            var builder = new BusConfiguration();
            builder.EndpointName("xyz");
            builder.TypesToScan(new[]
                {
                    typeof(MessageWithNonSerializableDataBusProperty)
                });
            builder.Conventions().DefiningDataBusPropertiesAs(p => p.Name.EndsWith("DataBus"));
            builder.RegisterComponents(c =>
            {
                c.RegisterSingleton<IDataBus>(new InMemoryDataBus());
                c.ConfigureComponent<IDataBusSerializer>(() => new MyDataBusSerializer(), DependencyLifecycle.SingleInstance);
            });

            var config = builder.BuildConfiguration();
            var feature = new DataBus();

            Assert.DoesNotThrow(() => feature.CheckPrerequisites(new FeatureConfigurationContext(config)));
        }

        class MyDataBusSerializer : IDataBusSerializer
        {
            public void Serialize(object databusProperty, Stream stream)
            {
                throw new NotImplementedException();
            }

            public object Deserialize(Stream stream)
            {
                throw new NotImplementedException();
            }
        }
    }


}
