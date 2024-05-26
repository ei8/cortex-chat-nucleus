using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters
{
    public class MatchExclude : IFilter
    {
        private readonly Guid[] neuronIds;

        public MatchExclude(params Guid[] neuronIds)
        {
            this.neuronIds = neuronIds;
        }

        public IEnumerable<Neuron> Evaluate(IEnumerable<Neuron> paths)
        {
            throw new NotImplementedException();
        }
    }
}
