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

        public DateTimeOffset? CreationTimestamp { get; set; }

        public DateTimeOffset? LastModificationTimestamp { get; set; }

        public Guid TerminalId { get; set; }
    }
}
