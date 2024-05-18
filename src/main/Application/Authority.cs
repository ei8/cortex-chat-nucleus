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

        /// <summary>
        /// Gets or sets the Avatars that use this Authority instance for user authentication.
        /// </summary>
        public IEnumerable<string> Avatars { get; set; }
    }
}
