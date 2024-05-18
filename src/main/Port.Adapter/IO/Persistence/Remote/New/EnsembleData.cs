using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public class EnsembleData
    {
        public EnsembleData(IEnumerable<NeuronData> neurons, IEnumerable<TerminalData> terminals)
        {
            this.Neurons = neurons;

            this.Terminals = terminals;
        }

        public IEnumerable<NeuronData> Neurons { get; private set; }
        public IEnumerable<TerminalData> Terminals { get; private set; }
    }
}
