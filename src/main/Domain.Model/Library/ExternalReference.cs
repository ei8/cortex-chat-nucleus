using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Library
{
    public class ExternalReference
    {
        public string Url { get; set; }
        public Guid Id { get; set; }
        public string Tag { get; set; }
    }
}
