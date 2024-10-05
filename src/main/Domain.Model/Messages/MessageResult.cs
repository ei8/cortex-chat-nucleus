using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    public class MessageResult
    {
        public MessageResult() {}

        public Message Message { get; set; }

        public string RegionTag { get; set; }

        public string SenderTag { get; set; }

        public bool IsCurrentUserSender { get; set; }
    }
}
