using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class DeclarationIdSet
    {
        public HeadIdSet Head { get; private set; } = new HeadIdSet();

        public SubordinationIdSet Subordination { get; private set; } =
            new SubordinationIdSet(typeof(SubordinationDependentIndex));

        public enum SubordinationDependentIndex
        {
            Dependent1
        }
    }
}
