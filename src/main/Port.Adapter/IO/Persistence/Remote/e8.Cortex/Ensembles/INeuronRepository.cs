using ei8.Cortex.Library.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public interface INeuronRepository
    {
        Task<IDictionary<string, Neuron>> GetExternalReferencesAsync(string userId, params string[] keys);

        Task<Neuron> GetByQueriesAsync(string userId, params NeuronQuery[] queries);
    }
}
