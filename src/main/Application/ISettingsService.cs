using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public interface ISettingsService
    {
        string CortexLibraryOutBaseUrl { get; }
        string EventSourcingInBaseUrl { get; }
        string EventSourcingOutBaseUrl { get; }
        string IdentityAccessInBaseUrl { get; }
        string IdentityAccessOutBaseUrl { get; }
        string InstantiatesMessageExternalReferenceUrl { get; }
        int PageSize { get; }
        IEnumerable<Authority> Authorities { get; set; }
    }
}
