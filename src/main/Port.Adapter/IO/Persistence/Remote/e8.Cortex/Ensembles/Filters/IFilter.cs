using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters
{
    public interface IFilter
    {
        IEnumerable<Neuron> Evaluate(IEnumerable<Neuron> neurons);
    }
}
