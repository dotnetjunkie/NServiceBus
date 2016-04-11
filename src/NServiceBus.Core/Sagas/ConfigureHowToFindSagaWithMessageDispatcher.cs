namespace NServiceBus.Sagas
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Saga;
    using Utils.Reflection;

    /// <summary>
    /// Class used to bridge the dependency between <see cref="Saga{T}"/> and <see cref="Configure"/>.
    /// </summary>
    class ConfigureHowToFindSagaWithMessageDispatcher : IConfigureHowToFindSagaWithMessage
    {
        readonly SagaConfigurationCache sagaConfigurationCache;

        public ConfigureHowToFindSagaWithMessageDispatcher(SagaConfigurationCache sagaConfigurationCache)
        {
            this.sagaConfigurationCache = sagaConfigurationCache;
        }

        void IConfigureHowToFindSagaWithMessage.ConfigureMapping<TSagaEntity, TMessage>(Expression<Func<TSagaEntity, object>> sagaEntityProperty, Expression<Func<TMessage, object>> messageExpression)
        {
            var sagaProp = Reflect<TSagaEntity>.GetProperty(sagaEntityProperty, true);
        
            ThrowIfNotPropertyLambdaExpression(sagaEntityProperty, sagaProp);
            var compiledMessageExpression = messageExpression.Compile();
            var messageFunc = new Func<object, object>(o => compiledMessageExpression((TMessage)o));

            var sagaToMessageMap = new SagaToMessageMap
            {
                MessageProp = messageFunc,
                SagaPropName = sagaProp.Name
            };
            sagaConfigurationCache.ConfigureHowToFindSagaWithMessage(typeof(TSagaEntity), typeof(TMessage), sagaToMessageMap);
        }

        // ReSharper disable once UnusedParameter.Local
        void ThrowIfNotPropertyLambdaExpression<TSagaEntity>(Expression<Func<TSagaEntity, object>> expression, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentException(
                    String.Format(
                        "Only public properties are supported for mapping Sagas. The lambda expression provided '{0}' is not mapping to a Property!",
                        expression.Body));
            }
        }
    }

    class SagaToMessageMap
    {
        public Func<object, object> MessageProp;
        public string SagaPropName;
    }
}