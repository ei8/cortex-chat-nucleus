using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Data
{
    public class EnsembleData
    {
        public EnsembleData(IEnumerable<NeuronData> neurons, IEnumerable<TerminalData> terminals)
        {
            Neurons = neurons;

            Terminals = terminals;
        }

        public IEnumerable<NeuronData> Neurons { get; private set; }
        public IEnumerable<TerminalData> Terminals { get; private set; }
    }
}
