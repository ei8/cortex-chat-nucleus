using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Data;
using ei8.Cortex.Library.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public interface IEnsembleService<TSelf, TParameterSet>
        where TSelf : IEnsembleService<TSelf, TParameterSet>
        where TParameterSet : IParameterSet
    {
        bool TryParse(Neuron ensemble, TParameterSet parameterSet, out Neuron result);

        IEnumerable<NeuronQuery> GetQueries(TParameterSet parameterSet);

        Task Build(EnsembleCollection ensembles, TParameterSet parameterSet, INeuronRepository neuronRepository, string userId);
    }
}
