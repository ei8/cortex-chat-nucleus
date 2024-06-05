using ei8.Cortex.Library.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public interface IEnsembleService<TSelf, TParameterSet>
        where TSelf : IEnsembleService<TSelf, TParameterSet>
        where TParameterSet : IParameterSet
    {
        bool TryParse(Ensemble ensemble, TParameterSet parameterSet, out Neuron result);

        IEnumerable<NeuronQuery> GetQueries(TParameterSet parameterSet);

        Task<Neuron> BuildAsync(Ensemble ensemble, TParameterSet parameterSet, INeuronRepository neuronRepository, string userId);
    }
}
