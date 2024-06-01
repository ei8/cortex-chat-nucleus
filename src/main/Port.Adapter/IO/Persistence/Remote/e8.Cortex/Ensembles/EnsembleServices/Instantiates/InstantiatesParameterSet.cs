using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.EnsembleServices
{
    public class InstantiatesParameterSet : IInstantiatesParameterSet
    {
        public InstantiatesParameterSet(
            Neuron @class
            )
        {
            Class = @class;
        }

        public Neuron Class { get; }
    }
}
