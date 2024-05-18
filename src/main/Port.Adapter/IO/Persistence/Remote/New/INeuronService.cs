using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public interface INeuronService
    {
        Task<IDictionary<string, Neuron>> GetExternalReferences(string userId, params string[] keys);
    }
}
