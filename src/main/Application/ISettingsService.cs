using ei8.EventSourcing.Port.Adapter.Common;
using System;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public interface ISettingsService
    {
        string CortexLibraryOutBaseUrl { get; }
        string EventSourcingInBaseUrl { get; }
        string EventSourcingOutBaseUrl { get; }
        string IdentityAccessInBaseUrl { get; }
        string IdentityAccessOutBaseUrl { get; }
        int PageSize { get; }
        IEnumerable<Authority> Authorities { get; }
        int QueryResultLimit { get; }
        string AppUserId { get; }
        bool InitializeMissingMirrors { get; }
        
        /// <summary>
        /// Gets the number of initialization retries 
        /// </summary>
        int InitializeRetryCount { get; }

        /// Gets the number of seconds to wait between initialization retries.
        /// </summary>
        int InitializeRetryDelay { get; }
    }
}
