using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    public class Recipient
    {
        // TODO:
        // public Guid Id { get; set; }
        public Guid MessageId { get; set; }        
        // TODO: Added temporarily, to be removed once a separate long-running process is responsible for updating statuses of messages once they are read by recipient avatar
        public Message Message { get; set; }
        public Guid AvatarId { get; set; }
        // TODO:
        // public StatusValue Status { get; set; }
    }
}
