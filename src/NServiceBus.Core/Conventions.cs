﻿namespace NServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NServiceBus.Utils.Reflection;

    /// <summary>
    ///     Message convention definitions.
    /// </summary>
    public class Conventions
    {
        internal IEnumerable<PropertyInfo> GetDataBusProperties(object message)
        {
            var messageType = message.GetType();


            List<PropertyInfo> value;

            if (!cache.TryGetValue(messageType, out value))
            {
                value = messageType.GetProperties()
                    .Where(IsDataBusProperty)
                    .ToList();

                cache[messageType] = value;
            }

            return value;
        }

        /// <summary>
        ///     Returns the time to be received for a give <paramref name="messageType" />.
        /// </summary>
        public TimeSpan GetTimeToBeReceived(Type messageType)
        {
            return TimeToBeReceivedAction(messageType);
        }

        /// <summary>
        ///     Returns true if the given type is a message type.
        /// </summary>
        public bool IsMessageType(Type t)
        {
            try
            {
                return MessagesConventionCache.ApplyConvention(t,
                    type =>
                    {
                        if (IsInSystemConventionList(type))
                        {
                            return true;
                        }
                        if (type.IsFromParticularAssembly())
                        {
                            return false;
                        }
                        return IsMessageTypeAction(type) ||
                               IsCommandTypeAction(type) ||
                               IsEventTypeAction(type);
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to evaluate Message convention. See inner exception for details.", ex);
            }
        }

        /// <summary>
        ///     Returns true is message is a system message type.
        /// </summary>
        public bool IsInSystemConventionList(Type t)
        {
            return IsSystemMessageActions.Any(isSystemMessageAction => isSystemMessageAction(t));
        }

        /// <summary>
        ///     Add system message convention
        /// </summary>
        /// <param name="definesMessageType">Function to define system message convention</param>
        public void AddSystemMessagesConventions(Func<Type, bool> definesMessageType)
        {
            if (!IsSystemMessageActions.Contains(definesMessageType))
            {
                IsSystemMessageActions.Add(definesMessageType);
                MessagesConventionCache.Reset();
            }
        }

        /// <summary>
        ///     Returns true if the given type is a command type.
        /// </summary>
        public bool IsCommandType(Type t)
        {
            try
            {
                return CommandsConventionCache.ApplyConvention(t, type =>
                {
                    if (type.IsFromParticularAssembly())
                    {
                        return false;
                    }
                    return IsCommandTypeAction(type);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to evaluate Command convention. See inner exception for details.", ex);
            }
        }

        /// <summary>
        ///     Returns true if the given type should not be written to disk when sent.
        /// </summary>
        public bool IsExpressMessageType(Type t)
        {
            try
            {
                return ExpressConventionCache.ApplyConvention(t, type =>
                {
                    if (type.IsFromParticularAssembly())
                    {
                        return false;
                    }
                    return IsExpressMessageAction(type);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to evaluate Express convention. See inner exception for details.", ex);
            }
        }

        /// <summary>
        ///     Returns true if the given property should be encrypted
        /// </summary>
        public bool IsEncryptedProperty(PropertyInfo property)
        {
            try
            {
                //the message mutator will cache the whole message so we don't need to cache here
                return IsEncryptedPropertyAction(property);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to evaluate Encrypted Property convention. See inner exception for details.", ex);
            }
        }

        /// <summary>
        ///     Returns true if the given property should be send via the DataBus
        /// </summary>
        public bool IsDataBusProperty(PropertyInfo property)
        {
            try
            {
                return IsDataBusPropertyAction(property);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to evaluate DataBus Property convention. See inner exception for details.", ex);
            }
        }

        /// <summary>
        ///     Returns true if the given type is a event type.
        /// </summary>
        public bool IsEventType(Type t)
        {
            try
            {
                return EventsConventionCache.ApplyConvention(t, type =>
                {
                    if (type.IsFromParticularAssembly())
                    {
                        return false;
                    }
                    return IsEventTypeAction(type);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to evaluate Event convention. See inner exception for details.", ex);
            }
        }

        readonly ConcurrentDictionary<Type, List<PropertyInfo>> cache = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        ConventionCache CommandsConventionCache = new ConventionCache();
        ConventionCache EventsConventionCache = new ConventionCache();
        ConventionCache ExpressConventionCache = new ConventionCache();

        internal Func<Type, bool> IsCommandTypeAction = t => typeof(ICommand).IsAssignableFrom(t) && typeof(ICommand) != t;

        internal Func<PropertyInfo, bool> IsDataBusPropertyAction = p => typeof(IDataBusProperty).IsAssignableFrom(p.PropertyType) && typeof(IDataBusProperty) != p.PropertyType;

        internal Func<PropertyInfo, bool> IsEncryptedPropertyAction = p => typeof(WireEncryptedString).IsAssignableFrom(p.PropertyType);

        internal Func<Type, bool> IsEventTypeAction = t => typeof(IEvent).IsAssignableFrom(t) && typeof(IEvent) != t;

        internal Func<Type, bool> IsExpressMessageAction = t => t.GetCustomAttributes(typeof(ExpressAttribute), true)
            .Any();

        internal Func<Type, bool> IsMessageTypeAction = t => typeof(IMessage).IsAssignableFrom(t) &&
                                                             typeof(IMessage) != t &&
                                                             typeof(IEvent) != t &&
                                                             typeof(ICommand) != t;

        List<Func<Type, bool>> IsSystemMessageActions = new List<Func<Type, bool>>();
        ConventionCache MessagesConventionCache = new ConventionCache();

        internal Func<Type, TimeSpan> TimeToBeReceivedAction = t =>
        {
            var attributes = t.GetCustomAttributes(typeof(TimeToBeReceivedAttribute), true)
                .Select(s => s as TimeToBeReceivedAttribute)
                .ToList();

            return attributes.Count > 0 ? attributes.Last().TimeToBeReceived : TimeSpan.MaxValue;
        };

        class ConventionCache
        {
            public bool ApplyConvention(Type type, Func<Type, bool> action)
            {
                return cache.GetOrAdd(type, action);
            }

            public void Reset()
            {
                cache.Clear();
            }

            ConcurrentDictionary<Type, bool> cache = new ConcurrentDictionary<Type, bool>();
        }
    }
}