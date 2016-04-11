namespace NServiceBus.Timeout.Core
{
    using System;
    using System.Collections.Generic;
    using Unicast;

    /// <summary>
    /// Holds timeout information.
    /// </summary>
    public class TimeoutData 
    {
        /// <summary>
        /// Id of this timeout
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The address of the client who requested the timeout.
        /// </summary>
        public Address Destination { get; set; }

        /// <summary>
        /// The saga ID.
        /// </summary>
        public Guid SagaId { get; set; }

        /// <summary>
        /// Additional state.
        /// </summary>
        public byte[] State { get; set; }

        /// <summary>
        /// The time at which the timeout expires.
        /// </summary>
        public DateTime Time { get; set; }
        
        /// <summary>
        /// The timeout manager that owns this particular timeout
        /// </summary>
        public string OwningTimeoutManager { get; set; }

        /// <summary>
        /// Store the headers to preserve them across timeouts
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents the current <see cref="Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("Timeout({0}) - Expires:{1}, SagaId:{2}", Id, Time, SagaId);
        }

        /// <summary>
        /// Transforms the timeout to a <see cref="TransportMessage"/>.
        /// </summary>
        /// <returns>Returns a <see cref="TransportMessage"/>.</returns>
        public TransportMessage ToTransportMessage()
        {
            var transportMessage = new TransportMessage(Id,Headers)
            {
                Recoverable = true,
                Body = State
            };


            if (SagaId != Guid.Empty)
            {
                transportMessage.Headers[NServiceBus.Headers.SagaId] = SagaId.ToString();
            }


            transportMessage.Headers["NServiceBus.RelatedToTimeoutId"] = Id;

            return transportMessage;
        }

        /// <summary>
        /// Transforms the timeout to send options.
        /// </summary>
        /// <param name="replyToAddress">The reply address to use for outgoing messages</param>
        public SendOptions ToSendOptions(Address replyToAddress)
        {
            if (Headers != null)
            {
                string originalReplyToAddressValue;
                if (Headers.TryGetValue(OriginalReplyToAddress, out originalReplyToAddressValue))
                {
                    replyToAddress = Address.Parse(originalReplyToAddressValue);
                    Headers.Remove(OriginalReplyToAddress);
                }
            }

            return new SendOptions(Destination)
            {
                ReplyToAddress = replyToAddress
            };
        }

        /// <summary>
        /// Original ReplyTo address header.
        /// </summary>
        public const string OriginalReplyToAddress = "NServiceBus.Timeout.ReplyToAddress";
    }
}
