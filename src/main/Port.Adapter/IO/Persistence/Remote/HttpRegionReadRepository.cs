using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpRegionReadRepository : IRegionReadRepository
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;
        private readonly IIdentityService identityService;

        public HttpRegionReadRepository(
            INeuronQueryClient neuronQueryClient,
            ISettingsService settingsService,
            IIdentityService identityService
            )
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(identityService, nameof(identityService));

            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
            this.identityService = identityService;
        }

        public async Task<IEnumerable<Region>> GetAll(CancellationToken token = default)
        {
            var neurons = await this.neuronQueryClient.GetNeuronsInternal(
                this.settingsService.CortexLibraryOutBaseUrl + "/",
                new NeuronQuery()
                {
                    PostsynapticExternalReferenceUrl = new string[] { this.settingsService.InstantiatesRegionExternalReferenceUrl },
                    SortBy = SortByValue.NeuronExternalReferenceUrl,
                    SortOrder = SortOrderValue.Ascending
                },
                this.identityService.UserId
                );

            return neurons.Items.Select(n => n.ToRegion());
        }

        public async Task<IEnumerable<Region>> GetByIds(IEnumerable<Guid> ids, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(ids, nameof(ids));

            var result = (IEnumerable<Region>) Array.Empty<Region>();

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
                    this.identityService.UserId
                    );

                result = neurons.Items.Select(n => n.ToRegion());
            }

            return result;
        }
    }
}
