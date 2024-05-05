using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class TerminalIdPair
    {
        public TerminalIdPair(Guid terminalId, Guid postsynapticNeuronId)
        {
            this.TerminalId = terminalId;

            this.PostsynapticNeuronId = postsynapticNeuronId;
        }

        public Guid TerminalId { get; private set; }

        public Guid PostsynapticNeuronId { get; private set; }
    }
}
