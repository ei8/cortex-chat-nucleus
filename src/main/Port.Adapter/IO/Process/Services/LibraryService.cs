using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;
        private readonly IIdentityService identityService;

        private IEnumerable<ExternalReference> externalReferences;
        private bool loaded;

        public LibraryService(INeuronQueryClient neuronQueryClient, ISettingsService settingsService, IIdentityService identityService)
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(identityService, nameof(identityService));

            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
            this.identityService = identityService;

            this.loaded = false;
        }

        public async Task<Guid> GetNeuronId(ExternalReferenceId value)
        {
            if (!this.loaded)
            {
                var sers = this.settingsService.ExternalReferences.ToArray();
                var erns = (await this.neuronQueryClient.GetNeuronsInternal(
                        this.settingsService.CortexLibraryOutBaseUrl + "/",
                        new NeuronQuery()
                        {
                            ExternalReferenceUrl = sers.Select(er => er.Url),
                            SortBy = SortByValue.NeuronCreationTimestamp,
                            SortOrder = SortOrderValue.Descending,
                            PageSize = sers.Length
                        },
                        // TODO: retrieve from settings
                        this.identityService.UserId
                    )).Items;

                for (int i = 0; i < sers.Length; i++)
                {
                    var ern = erns.SingleOrDefault(n => n.ExternalReferenceUrl == sers[i].Url);
                    AssertionConcern.AssertStateTrue(ern != null, $"Local copy of External Reference neuron '{sers[i].Id}' was not found.");
                    sers[i].NeuronId = Guid.Parse(ern.Id);
                }
                this.externalReferences = sers;
                this.loaded = true;
            }

            return this.externalReferences.Single(er => er.Id == value).NeuronId;
        }
    }
}
    