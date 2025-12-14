using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages.Commands
{
    /// <summary>
    /// Represents the CreateMessage command.
    /// </summary>
    public class CreateMessage : ICommand
    {
        /// <summary>
        /// Constructs a CreateMessage command.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="regionId"></param>
        /// <param name="mirrorUrl"></param>
        /// <param name="recipientAvatarIds"></param>
        /// <param name="userId"></param>
        public CreateMessage(
            Guid id, 
            string content, 
            Guid? regionId, 
            string mirrorUrl, 
            IEnumerable<Guid> recipientAvatarIds, 
            string userId
        )
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(id)
                );
            AssertionConcern.AssertArgumentNotNull(content, nameof(content));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                regionId,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(regionId)
                );
            AssertionConcern.AssertArgumentNotNull(recipientAvatarIds, nameof(recipientAvatarIds));
            AssertionConcern.AssertArgumentNotEmpty(userId, Constants.Exception.InvalidUserId, nameof(userId));

            this.Id = id;
            this.Content = content;
            this.RegionId = regionId;
            this.MirrorUrl = mirrorUrl;
            this.RecipientAvatarIds = recipientAvatarIds;
            this.UserId = userId;
        }

        /// <summary>
        /// Gets the ID.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the Content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the RegionId.
        /// </summary>
        public Guid? RegionId { get; private set; }

        /// <summary>
        /// Gets the MirrorUrl.
        /// </summary>
        public string MirrorUrl { get; private set; }

        /// <summary>
        /// Gets the RecipientAvatarIds.
        /// </summary>
        public IEnumerable<Guid> RecipientAvatarIds { get; private set; }

        /// <summary>
        /// Gets the UserId.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Gest the ExpectedVersion.
        /// </summary>
        public int ExpectedVersion { get; private set; }
    }
}
