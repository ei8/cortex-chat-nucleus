using ei8.Cortex.Library.Common;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters
{
    public class Relation : IFilter
    {
        private readonly RelativeType type;

        public Relation(RelativeType type)
        {
            this.type = type;
        }

        public IEnumerable<Neuron> Evaluate(Ensemble ensemble, IEnumerable<Neuron> paths)
        {
            throw new System.NotImplementedException();
        }
    }
}
