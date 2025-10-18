using ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding.Reflection;
using ei8.Cortex.Coding.Versioning;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Common
{
    public struct EnvironmentVariableKeys
    {
        public const string CortexLibraryOutBaseUrl = "CORTEX_LIBRARY_OUT_BASE_URL";
        public const string EventSourcingInBaseUrl = "EVENT_SOURCING_IN_BASE_URL";
        public const string EventSourcingOutBaseUrl = "EVENT_SOURCING_OUT_BASE_URL";
        public const string IdentityAccessInBaseUrl = "IDENTITY_ACCESS_IN_BASE_URL";
        public const string IdentityAccessOutBaseUrl = "IDENTITY_ACCESS_OUT_BASE_URL";
        public const string PageSize = "PAGE_SIZE";
        public const string QueryResultLimit = "QUERY_RESULT_LIMIT";
        public const string AppUserId = "APP_USER_ID";
        public const string InitializeMissingMirrors = "INITIALIZE_MISSING_MIRRORS";
    }

    public readonly struct Constants
    {
        public static readonly IEnumerable<object> InitMirrorKeys = ReflectionExtensions.GetMirrorKeys(
            typeof(Avatar),
            typeof(Message),
            typeof(Creation),
            typeof(Sender),
            typeof(Recipient)
        );
    }

    public struct Default
    {
        public const int PageSize = 20;
        public const int QueryResultLimit = 10;
    }
}
