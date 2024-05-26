using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.neurULization
{
    public class neurULizationOptions
    {
        public Func<string[], Task<IDictionary<string, Neuron>>> ExternalReferenceRetriever { get; set; }
    }
}
