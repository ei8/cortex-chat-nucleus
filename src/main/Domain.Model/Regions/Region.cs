using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars
{
    public class Region
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ExternalReferenceUrl { get; set; }
    }
}
