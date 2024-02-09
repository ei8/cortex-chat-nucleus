using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    public class Destination
    {
        // TODO:
        // public Guid Id { get; set; }
        public Guid MessageId { get; set; }        
        // TODO: Added temporarily, to be removed once a separate long-running process is responsible for sending messages and updating statuses
        public Message Message { get; set; }
        public Guid RegionId { get; set; }
        // TODO:
        // public StatusValue Status { get; set; }
    }
}
