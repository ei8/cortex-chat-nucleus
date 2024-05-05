using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class PropertyIdSet
    {
        public DeclarationIdSet Declaration { get; private set; } = new DeclarationIdSet();

        public AssignmentIdSet Assignment { get; private set; } = new AssignmentIdSet();

        public AssociationIdSet Association { get; private set; } = new AssociationIdSet();
    }
}
