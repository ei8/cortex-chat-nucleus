using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
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
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;

        public HttpMessageReadRepository(INeuronQueryClient neuronQueryClient,
            ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
        }

        public async Task<IEnumerable<Message>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, string userId, CancellationToken token = default)
        {
            if (!maxTimestamp.HasValue)
                maxTimestamp = DateTimeOffset.UtcNow;

            if (!pageSize.HasValue)
                pageSize = this.settingsService.PageSize;

            var result = await this.neuronQueryClient.GetNeuronsInternal(
                this.settingsService.CortexLibraryOutBaseUrl + "/",
                new NeuronQuery()
                {
                    PostsynapticExternalReferenceUrl = new string[] { this.settingsService.InstantiatesMessageExternalReferenceUrl },
                    SortBy = SortByValue.NeuronCreationTimestamp,
                    SortOrder = SortOrderValue.Descending
                },
                userId
                );

            return result.Items
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
                    UnifiedLastModificationTimestamp = 
                        DateTimeOffset.TryParse(
                            n.UnifiedLastModification.Timestamp, 
                            out DateTimeOffset unifiedLastModification) ? 
                                (DateTimeOffset?)unifiedLastModification : 
                                null,
                    IsCurrentUserCreationAuthor = n.Validation.IsCurrentUserCreationAuthor
                });
        }
    }
}
