using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex
{
    public interface INeuronService
    {
        Task<IDictionary<string, Neuron>> GetExternalReferences(string userId, params string[] keys);

        Task<IDictionary<string, Neuron>> GetInstantiatesClassNeurons(string userId, params string[] keys);
    }
}
