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

        public string ExternalReferenceUrl { get; set; }

        public DateTimeOffset? CreationTimestamp { get; set; }

        public DateTimeOffset? UnifiedLastModificationTimestamp { get; set; }

        // TODO: remove since it is not part of model and only needed by Repo.Save?
        public Guid InstantiatesMessageTerminalId { get; set; }
    }
}
