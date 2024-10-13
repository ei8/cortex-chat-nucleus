using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages.Commands
{
    public class CreateMessage : ICommand
    {
        public CreateMessage(Guid id, string content, Guid? regionId, string externalReferenceUrl, IEnumerable<Guid> recipientAvatarIds, string userId)
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
            AssertionConcern.AssertArgumentNotNull(content, nameof(content));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                regionId,
                Messages.Exception.InvalidId,
                nameof(regionId)
                );
            AssertionConcern.AssertArgumentNotEmpty(userId, Messages.Exception.InvalidUserId, nameof(userId));

            this.Id = id;            
            this.Content = content;
            this.RegionId = regionId;
            this.ExternalReferenceUrl = externalReferenceUrl;
            this.RecipientAvatarIds = recipientAvatarIds;
            this.UserId = userId;
        }

        public Guid Id { get; private set; }
        
        public string Content { get; private set; }

        public Guid? RegionId { get; private set; }

        public string ExternalReferenceUrl { get; private set; }

        public IEnumerable<Guid> RecipientAvatarIds { get; private set; }

        public string UserId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
