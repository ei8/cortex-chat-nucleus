using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.Common;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IEnumerable<Authority> authorities;

        public SettingsService(IOptions<List<Authority>> authorities)
        {
            this.authorities = authorities.Value.ToArray();
        }

        public string CortexLibraryOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CortexLibraryOutBaseUrl);

        public string EventSourcingInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.EventSourcingInBaseUrl);

        public string EventSourcingOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.EventSourcingOutBaseUrl);

        public string IdentityAccessInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.IdentityAccessInBaseUrl);

        public string IdentityAccessOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.IdentityAccessOutBaseUrl);

        public int PageSize => int.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PageSize), out int size) ? size : Default.PageSize;

        public IEnumerable<Authority> Authorities => this.authorities;

        public int QueryResultLimit => int.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.QueryResultLimit), out int size) ? size : Default.QueryResultLimit;

        public string AppUserId => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.AppUserId);

        public bool InitializeMissingMirrors => 
            bool.TryParse(
                Environment.GetEnvironmentVariable(EnvironmentVariableKeys.InitializeMissingMirrors), 
                out bool create
            ) && 
            create;

        /// <summary>
        /// Gets the number of initialization retries 
        /// </summary>
        public int InitializeRetryCount => int.TryParse(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.InitializeRetryCount), 
            out int retryCount
        ) ? 
            retryCount : 
            Default.InitializeRetryCount;

        /// <summary>
        /// Gets the number of seconds to wait between initialization retries.
        /// </summary>
        public int InitializeRetryDelay => int.TryParse(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.InitializeRetryDelay),
            out int retryDelay
        ) ?
            retryDelay :
            Default.InitializeRetryDelay;
    }
}
