using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Data;
using ei8.Cortex.Library.Common;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public interface IEnsemble<TSelf, TEnsembleIdProvider>
        where TSelf : IEnsemble<TSelf, TEnsembleIdProvider>
        where TEnsembleIdProvider : IEnsembleIdProvider
    {
        Neuron Neuron { get; }

        bool TryParse(EnsembleData value, TEnsembleIdProvider ensembleIdProvider, out TSelf result);

        IEnumerable<NeuronQuery> GetDataQueries(TEnsembleIdProvider ensembleIdProvider);
    }
}
