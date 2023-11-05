using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Graph.Client;
using ei8.Cortex.Graph.Common;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageReadRepository : IMessageReadRepository
    {
        private readonly INeuronGraphQueryClient neuronGraphQueryClient;
        private readonly ISettingsService settingsService;

        public HttpMessageReadRepository(INeuronGraphQueryClient neuronGraphQueryClient,
            ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(neuronGraphQueryClient, nameof(neuronGraphQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.neuronGraphQueryClient = neuronGraphQueryClient;
            this.settingsService = settingsService;
        }

        public async Task<IEnumerable<Message>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, CancellationToken token = default)
        {
            if (!maxTimestamp.HasValue)
                maxTimestamp = DateTimeOffset.UtcNow;

            if (!pageSize.HasValue)
                pageSize = this.settingsService.PageSize;

            var result = await this.neuronGraphQueryClient.GetNeurons(
                this.settingsService.CortexGraphOutBaseUrl + "/",
                new NeuronQuery()
                {
                    PostsynapticExternalReferenceUrl = new string[] { this.settingsService.InstantiatesMessageExternalReferenceUrl },
                    SortBy = SortByValue.NeuronCreationTimestamp,
                    SortOrder = SortOrderValue.Descending
                });

            return result.Neurons
                .Where(nr => DateTimeOffset.TryParse(nr.Creation.Timestamp, out DateTimeOffset currentCreationTimestamp) && currentCreationTimestamp <= maxTimestamp)
                .Take(pageSize.Value)
                .Reverse()
                .Select(n => new Message()
                {
                    Id = Guid.Parse(n.Id),
                    Content = n.Tag,
                    Region = n.Region?.Tag,
                    RegionId = Guid.TryParse(n.Region?.Id, out Guid newRegionId) ? (Guid?)newRegionId : null,
                    Sender = n.Creation.Author.Tag,
                    SenderId = Guid.Parse(n.Creation.Author.Id),
                    CreationTimestamp = DateTimeOffset.TryParse(n.Creation.Timestamp, out DateTimeOffset creation) ? (DateTimeOffset?)creation : null,
                    LastModificationTimestamp = DateTimeOffset.TryParse(n.Creation.Timestamp, out DateTimeOffset lastModification) ? (DateTimeOffset?)lastModification : null
                });
        }

    }
}
