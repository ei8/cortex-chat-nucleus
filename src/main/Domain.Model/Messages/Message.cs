using System;
using System.Collections;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    public class Message
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public Guid? RegionId { get; set; }

        public Guid SenderId { get; set; }

        public string UserId { get; set; }

        public string ExternalReferenceUrl { get; set; }

        public IEnumerable<Guid> DestinationRegionIds { get; set; }

        public DateTimeOffset? CreationTimestamp { get; set; }

        public DateTimeOffset? UnifiedLastModificationTimestamp { get; set; }

        public Guid InstantiatesMessageTerminalId { get; set; }
    }
}
