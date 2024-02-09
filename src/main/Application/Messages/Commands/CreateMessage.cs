using CQRSlite.Commands;
using Newtonsoft.Json;
using neurUL.Common.Domain.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages.Commands
{
    public class CreateMessage : ICommand
    {
        public CreateMessage(Guid id, string content, Guid? regionId, string externalReferenceUrl, IEnumerable<Guid> destinationRegionIds)
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
            AssertionConcern.AssertArgumentNotNull(destinationRegionIds, nameof(destinationRegionIds));

            this.Id = id;            
            this.Content = content;
            this.RegionId = regionId;
            this.ExternalReferenceUrl = externalReferenceUrl;
            this.DestinationRegionIds = destinationRegionIds;
        }

        public Guid Id { get; private set; }
        
        public string Content { get; private set; }

        public Guid? RegionId { get; private set; }

        public string ExternalReferenceUrl { get; private set; }

        public IEnumerable<Guid> DestinationRegionIds { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
