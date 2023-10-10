﻿using System;
using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.Common;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services
{
    public class SettingsService : ISettingsService
    {
        public string CortexGraphOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CortexGraphOutBaseUrl);

        public string EventSourcingInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.EventSourcingInBaseUrl);

        public string EventSourcingOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.EventSourcingOutBaseUrl);

        public string IdentityAccessInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.IdentityAccessInBaseUrl);

        public string IdentityAccessOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.IdentityAccessOutBaseUrl);
        
        public string SubscriptionsInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.SubscriptionsInBaseUrl);
        
        public string SubscriptionsOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.SubscriptionsOutBaseUrl);

        public string InstantiatesMessageExternalReferenceUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.InstantiatesMessageExternalReferenceUrl);

        public int PageSize => int.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PageSize), out int size) ? size : Default.PageSize;
    }
}
