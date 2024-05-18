using neurUL.Cortex.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public class TerminalData
    {
        public Guid Id { get; set; }

        public Guid PresynapticNeuronId { get; set; }

        public Guid PostsynapticNeuronId { get; set; }

        public NeurotransmitterEffect Effect { get; set; }

        public float Strength { get; set; }
    }
}
