using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class AssignmentIdSet
    {
        public DependentIdSet Dependent { get; private set; } = new DependentIdSet();

        public SubordinationIdSet Subordination { get; private set; } = 
            new SubordinationIdSet(typeof(SubordinationDependentIndex));

        public enum SubordinationDependentIndex
        {
            Dependent1
        }
    }
}
