using ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars;
using ei8.Cortex.Coding.Model;
using ei8.Cortex.Coding.Model.Properties;
using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Represents a Message.
    /// </summary>
    public class Message : CreatedInstanceBase
    {
        public Message()
        {
        }

        public Message(Guid id, Guid contentId, DateTimeOffset? creationTimestamp)
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(id)
            );
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                contentId,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(contentId)
            );
            AssertionConcern.AssertArgumentNotNull(
                creationTimestamp.GetValueOrDefault(),
                nameof(creationTimestamp)
            );

            this.Id = id;
            this.ContentId = contentId;
            this.CreationTimestamp = creationTimestamp;
        }

        /// <summary>
        /// Gets or sets the ID of the message content.
        /// </summary>
        [neurULClass]
        public Guid ContentId { get; set; }
    }
}
