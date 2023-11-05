using CQRSlite.Commands;
using Newtonsoft.Json;
using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages.Commands
{
    public class CreateMessage : ICommand
    {
        public CreateMessage(Guid id, string content, Guid? regionId, string userId)
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
            AssertionConcern.AssertArgumentNotEmpty(
                userId,
                Messages.Exception.InvalidUserId,
                nameof(userId)
                );

            this.Id = id;            
            this.Content = content;
            this.RegionId = regionId;
            this.UserId = userId;
        }

        public Guid Id { get; private set; }
        
        public string Content { get; private set; }

        public Guid? RegionId { get; private set; }

        public string UserId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
