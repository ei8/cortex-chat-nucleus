using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public class Authority
    {
        public string Address { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public IEnumerable<string> Avatars { get; set; }
    }
}
