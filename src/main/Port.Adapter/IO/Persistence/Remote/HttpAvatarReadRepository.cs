using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using Microsoft.Extensions.Options;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpAvatarReadRepository : IAvatarReadRepository
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;
        private readonly IEnumerable<ExternalReference> externalReferences;

        public HttpAvatarReadRepository(
            INeuronQueryClient neuronQueryClient,
            ISettingsService settingsService,
            IOptions<List<ExternalReference>> externalReferences
            )
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(externalReferences, nameof(externalReferences));
            
            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
            this.externalReferences = externalReferences.Value.ToArray();
        }

        public async Task<IEnumerable<Avatar>> GetAll(string userId, CancellationToken token = default)
        {
            var neurons = await this.neuronQueryClient.GetNeuronsInternal(
                this.settingsService.CortexLibraryOutBaseUrl + "/",
                new NeuronQuery()
                {
                    PostsynapticExternalReferenceUrl = this.externalReferences.Where(er => er.Key == ExternalReferenceKey.InstantiatesAvatar.ToString()).Select(er => er.Url),
                    SortBy = SortByValue.NeuronExternalReferenceUrl,
                    SortOrder = SortOrderValue.Ascending
                },
                userId
                );

            return neurons.Items.Select(n => n.ToDomainAvatar());
        }

        public async Task<IEnumerable<Avatar>> GetByIds(IEnumerable<Guid> ids, string userId, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(ids, nameof(ids));

            var result = (IEnumerable<Avatar>) Array.Empty<Avatar>();

            if (ids.Any())
            {
                var neurons = await this.neuronQueryClient.GetNeuronsInternal(
                    this.settingsService.CortexLibraryOutBaseUrl + "/",
                    new NeuronQuery()
                    {
                        Id = ids.Select(i => i.ToString()),
                        SortBy = SortByValue.NeuronExternalReferenceUrl,
                        SortOrder = SortOrderValue.Ascending
                    },
                    userId
                    );

                result = neurons.Items.Select(n => n.ToDomainAvatar());
            }

            return result;
        }
    }
}
