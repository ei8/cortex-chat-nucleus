using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public class NeuronData
    {
        public Guid Id { get; set; }
        public string Tag { get; set; }
        public string ExternalReferenceUrl { get; set; }
        public Guid? RegionId { get; set; }
    }
}
