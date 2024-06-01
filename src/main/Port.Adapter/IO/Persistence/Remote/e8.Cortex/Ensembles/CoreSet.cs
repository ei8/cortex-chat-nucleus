using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class CoreSet : ICoreSet
    {
        public CoreSet()
        {            
        }

        public Neuron DirectObject { get; set; }

        public Neuron Subordination { get; set; }

        public Neuron InstantiatesUnit { get; set; }
    }
}
