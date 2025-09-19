using ei8.Cortex.Coding.Properties;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Represents a Message.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        [neurULNeuronProperty]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the message content.
        /// </summary>
        [neurULClass]
        public Guid ContentId { get; set; }
    }
}
