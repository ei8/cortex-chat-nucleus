using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public class Message
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public string Region { get; set; }

        public Guid? RegionId { get; set; }

        public string Sender { get; set; }

        public Guid SenderId { get; set; }

        public string UserId { get; set; }

        public DateTimeOffset? CreationTimestamp { get; set; }

        public DateTimeOffset? UnifiedLastModificationTimestamp { get; set; }

        public Guid InstantiatesMessageTerminalId { get; set; }

        public bool IsCurrentUserCreationAuthor { get; set; }
    }
}
