using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class DependentIdSet
    {
        public Guid NeuronId { get; private set; } = Guid.NewGuid();

        public Guid DependentTerminalId { get; private set; } = Guid.NewGuid();

        public Guid IdeaTerminalId { get; private set; } = Guid.NewGuid();
    }
}
