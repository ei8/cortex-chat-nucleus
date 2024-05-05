using System;
using System.Collections.Generic;
using System.Text;
using static ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.AssignmentIdSet;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class SubordinationIdSet
    {
        public SubordinationIdSet(Type enumType) : this(Enum.GetNames(enumType).Length)
        {            
        }

        public SubordinationIdSet(int dependentCount)
        {
            this.DependentCount = dependentCount;
            var list = new List<Guid>(dependentCount);
            for (int i = 0; i < dependentCount; i++) list.Add(Guid.NewGuid());
            this.DependentTerminalIds = list.ToArray();
        }

        public Guid NeuronId { get; private set; } = Guid.NewGuid();
        public Guid SubordinationTerminalId { get; private set; } = Guid.NewGuid();
        public Guid HeadTerminalId { get; private set; } = Guid.NewGuid();
        public Guid[] DependentTerminalIds { get; private set; }
        public int DependentCount { get; private set; }
    }
}
