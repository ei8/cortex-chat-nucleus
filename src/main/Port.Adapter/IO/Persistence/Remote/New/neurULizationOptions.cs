using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public class neurULizationOptions
    {
        public Func<string[], Task<IDictionary<string, Neuron>>> ExternalReferenceRetriever { get; set; }
    }
}
