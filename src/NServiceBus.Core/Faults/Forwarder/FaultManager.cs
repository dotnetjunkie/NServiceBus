namespace NServiceBus.Faults.Forwarder
{
    using System;
    using Logging;
    using ObjectBuilder;
    using SecondLevelRetries.Helpers;
    using Transports;
    using Unicast;
    using Unicast.Queuing;

    /// <summary>
    /// Implementation of IManageMessageFailures by forwarding messages
    /// using ISendMessages.
    /// </summary>
    class FaultManager : IManageMessageFailures
    {
        readonly IBuilder builder;
        readonly Configure config;

        public FaultManager(IBuilder builder, Configure config)
        {
            this.builder = builder;
            this.config = config;
        }

        void IManageMessageFailures.SerializationFailedForMessage(TransportMessage message, Exception e)
        {
            SendFailureMessage(message, e, true);
        }

        void IManageMessageFailures.ProcessingAlwaysFailsForMessage(TransportMessage message, Exception e)
        {
            SendFailureMessage(message, e);
        }

        void IManageMessageFailures.Init(Address address)
        {
            localAddress = address;
        }

        void SendFailureMessage(TransportMessage message, Exception e, bool serializationException = false)
        {
            message.SetExceptionHeaders(e, localAddress ?? config.LocalAddress);

            try
            {
                var destinationQ = RetriesErrorQueue ?? ErrorQueue;
               
                // Intentionally service-locate ISendMessages to avoid circular
                // resolution problem in the container
                var sender = builder.Build<ISendMessages>();

                if (serializationException || MessageWasSentFromSLR(message))
                {
                    sender.Send(message, new SendOptions(ErrorQueue));
                    return;
                }

                sender.Send(message, new SendOptions(destinationQ));

                //HACK: We need this hack here till we refactor the SLR to be a first class concept in the TransportReceiver
                if (RetriesErrorQueue == null)
                {
                    Logger.ErrorFormat("Message with '{0}' id has failed FLR and will be moved to the configured error queue.", message.Id);
                }
                else
                {
                    var retryAttempt = TransportMessageHeaderHelper.GetNumberOfRetries(message) + 1;

                    Logger.WarnFormat("Message with '{0}' id has failed FLR and will be handed over to SLR for retry attempt {1}.", message.Id, retryAttempt);
                }
            }
            catch (Exception exception)
            {
                var queueNotFoundException = exception as QueueNotFoundException;
                string errorMessage;

                if (queueNotFoundException != null)
                {
                    errorMessage = string.Format("Could not forward failed message to error queue '{0}' as it could not be found.", queueNotFoundException.Queue);
                    Logger.Fatal(errorMessage);
                }
                else
                {
                    errorMessage = "Could not forward failed message to error queue.";
                    Logger.Fatal(errorMessage, exception);
                }

                throw new InvalidOperationException(errorMessage, exception);
            }
        }

        bool MessageWasSentFromSLR(TransportMessage message)
        {
            if (RetriesErrorQueue == null)
            {
                return false;
            }

            // if the reply to address == ErrorQueue and RealErrorQueue is not null, the
            // SecondLevelRetries sat is running and the error happened within that sat.            
            return TransportMessageHeaderHelper.GetAddressOfFaultingEndpoint(message) == RetriesErrorQueue;
        }

        /// <summary>
        /// Endpoint to which message failures are forwarded
        /// </summary>
        public Address ErrorQueue { get; set; }

        /// <summary>
        /// The address of the Second Level Retries input queue when SLR is enabled
        /// </summary>
        public Address RetriesErrorQueue { get; set; }

        Address localAddress;
        static ILog Logger = LogManager.GetLogger<FaultManager>();
    }
}
