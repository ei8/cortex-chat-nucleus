using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public const string CreateMirrorsIfNotFound = "CREATE_EXTERNAL_REFERENCES_IF_NOT_FOUND";
    }

    public struct Default
    {
        public const int PageSize = 20;
        public const int QueryResultLimit = 10;
        public static readonly IEnumerable<object> InitMirrorKeys =
                new object[] { 
                    typeof(Avatar),
                    typeof(Avatar).GetProperty(nameof(Avatar.Name)),
                    typeof(Avatar).GetProperty(nameof(Avatar.CreationTimestamp)),
                    typeof(Avatar).GetProperty(nameof(Avatar.LastModificationTimestamp))
                }
                .Concat(new object[] {
                    typeof(Message),
                    typeof(Message).GetProperty(nameof(Message.ContentId)),
                    typeof(Message).GetProperty(nameof(Message.SenderId)),
                    typeof(Message).GetProperty(nameof(Message.CreationTimestamp)),
                    typeof(Message).GetProperty(nameof(Message.LastModificationTimestamp))
                });
    }
}
