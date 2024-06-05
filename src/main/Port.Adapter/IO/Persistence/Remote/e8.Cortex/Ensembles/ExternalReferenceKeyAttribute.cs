using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class ExternalReferenceKeyAttribute : Attribute
    {
        public ExternalReferenceKeyAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; private set; }
    }
}
