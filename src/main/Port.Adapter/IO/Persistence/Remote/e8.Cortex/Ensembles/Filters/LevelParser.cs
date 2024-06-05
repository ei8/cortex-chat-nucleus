using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters
{
    public class LevelParser
    {
        public LevelParser(params IFilter[] filters)
        {
            Filters = filters;
        }

        public IEnumerable<IFilter> Filters { get; private set; }

        public IEnumerable<Neuron> Evaluate(Ensemble ensemble, IEnumerable<Neuron> neurons)
        {
            foreach (var filter in Filters)
                neurons = filter.Evaluate(ensemble, neurons);

            return neurons;
        }
    }
}
