using ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars;
using ei8.Cortex.Coding.Model;
using ei8.Cortex.Coding.Model.Properties;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Represents the base class for Communicators.
    /// </summary>
    public abstract class CommunicatorBase : InstanceBase
    {
        /// <summary>
        /// Gets or sets the Message related to the Communicator.
        /// </summary>
        [neurULClass(typeof(Message))]
        public Guid MessageId { get; set; }

        /// <summary>
        /// Gets or sets the Avatar of the Communicator.
        /// </summary>
        [neurULClass(typeof(Avatar))]
        public Guid AvatarId { get; set; }
    }
}
