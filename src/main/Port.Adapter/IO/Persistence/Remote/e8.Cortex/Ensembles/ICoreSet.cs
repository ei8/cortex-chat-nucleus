using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public interface ICoreSet
    {
        Neuron DirectObject { get; }

        Neuron Subordination { get; }

        Neuron InstantiatesUnit { get; }
    }
}
