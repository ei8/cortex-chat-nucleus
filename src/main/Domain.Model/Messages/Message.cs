using ei8.Cortex.Coding.Model;
using ei8.Cortex.Coding.Model.Properties;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Represents a Message.
    /// </summary>
    public class Message : CreatedInstanceBase
    {
        /// <summary>
        /// Gets or sets the ID of the message content.
        /// </summary>
        [neurULClass]
        public Guid ContentId { get; set; }
    }
}
