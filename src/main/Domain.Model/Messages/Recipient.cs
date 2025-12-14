using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Represents a Recipient.
    /// </summary>
    public class Recipient : CommunicatorBase
    {
        public Recipient()
        {
        }

        public Recipient(Guid id, Guid messageId, Guid avatarId)
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(id)
            );
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                messageId,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(messageId)
            );
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                avatarId,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(avatarId)
            );

            this.Id = id;
            this.MessageId = messageId;
            this.AvatarId = avatarId;
        }
}
}
