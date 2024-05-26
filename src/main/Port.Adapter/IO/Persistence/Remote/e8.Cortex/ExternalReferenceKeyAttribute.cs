using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex
{
    public class ExternalReferenceKeyAttribute : Attribute
    {
        public ExternalReferenceKeyAttribute(string key)
        {
            this.Key = key;
        }

        public string Key { get; private set; }
    }
}
