using System;
using System.Collections;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Property names based on function in Domain.Model.Library.TagValues
    /// </summary>
    public class Message
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public Guid? RegionId { get; set; }

        public Guid SenderId { get; set; }

        public string ExternalReferenceUrl { get; set; }

        public DateTimeOffset? CreationTimestamp { get; set; }

        public DateTimeOffset? UnifiedLastModificationTimestamp { get; set; }
    }
}
