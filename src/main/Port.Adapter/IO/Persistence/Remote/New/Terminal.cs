using neurUL.Cortex.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public class Terminal
    {
        internal Terminal() { }

        public Guid Id { get; set; }
        public bool IsTransient { get; internal set; }
        public float Strength { get; set; }
        public NeurotransmitterEffect Effect { get; set; }
        public Neuron Presynaptic { get; internal set; }
        public Neuron Postsynaptic { get; internal set; }
    }
}
