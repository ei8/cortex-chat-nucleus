using ei8.Cortex.Coding.Properties;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Represents the base class for Communicators.
    /// </summary>
    public abstract class CommunicatorBase
    {
        /// <summary>
        /// Gets or sets the Id of the Communicator.
        /// </summary>
        [neurULNeuronProperty]
        public Guid Id { get; set; }

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
