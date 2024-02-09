using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IConfiguration configuration;
        public SettingsService(IConfiguration configuration)
        {
            this.configuration = configuration;            
        }

        public string CortexLibraryOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CortexLibraryOutBaseUrl);

        public string EventSourcingInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.EventSourcingInBaseUrl);

        public string EventSourcingOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.EventSourcingOutBaseUrl);

        public string IdentityAccessInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.IdentityAccessInBaseUrl);

        public string IdentityAccessOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.IdentityAccessOutBaseUrl);

        public string InstantiatesMessageExternalReferenceUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.InstantiatesMessageExternalReferenceUrl);

        public string InstantiatesRegionExternalReferenceUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.InstantiatesRegionExternalReferenceUrl);

        public int PageSize => int.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PageSize), out int size) ? size : Default.PageSize;

        public IEnumerable<Authority> Authorities => this.configuration.GetSection(nameof(Authorities)).Get<IEnumerable<Authority>>();
    }
}
