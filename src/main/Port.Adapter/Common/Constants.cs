using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Common
{
    public struct EnvironmentVariableKeys
    {
        public const string CortexGraphOutBaseUrl = "CORTEX_GRAPH_OUT_BASE_URL";
        public const string EventSourcingInBaseUrl = "EVENT_SOURCING_IN_BASE_URL";
        public const string EventSourcingOutBaseUrl = "EVENT_SOURCING_OUT_BASE_URL";
        public const string IdentityAccessInBaseUrl = "IDENTITY_ACCESS_IN_BASE_URL";
        public const string IdentityAccessOutBaseUrl = "IDENTITY_ACCESS_OUT_BASE_URL";
        public const string InstantiatesMessageExternalReferenceUrl = "INSTANTIATES_MESSAGE_EXTERNAL_REFERENCE_URL";
        public const string PageSize = "PAGE_SIZE";
    }

    public struct Default
    {
        public const int PageSize = 20;
    }
}
