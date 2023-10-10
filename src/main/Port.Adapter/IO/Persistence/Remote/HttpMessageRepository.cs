using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Graph.Client;
using ei8.Cortex.Graph.Common;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public class HttpMessageRepository : IMessageRepository
    {
        private readonly ITransaction transaction;
        private readonly IAuthoredEventStore eventStore;
        private readonly IInMemoryAuthoredEventStore inMemoryEventStore;
        private readonly INeuronGraphQueryClient neuronGraphQueryClient;
        private readonly INeuronAdapter neuronAdapter;
        private readonly ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter;
        private readonly ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter;
        private readonly ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter;
        private readonly ISettingsService settingsService;
        
        public HttpMessageRepository(
            ITransaction transaction,
            IAuthoredEventStore eventStore,
            IInMemoryAuthoredEventStore inMemoryEventStore,
            INeuronAdapter neuronAdapter,
            ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter,
            ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter,
            ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter,
            INeuronGraphQueryClient neuronGraphQueryClient,            
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(eventStore, nameof(eventStore));
            AssertionConcern.AssertArgumentNotNull(inMemoryEventStore, nameof(inMemoryEventStore));
            AssertionConcern.AssertArgumentNotNull(neuronAdapter, nameof(neuronAdapter));
            AssertionConcern.AssertArgumentNotNull(tagItemAdapter, nameof(tagItemAdapter));
            AssertionConcern.AssertArgumentNotNull(aggregateItemAdapter, nameof(aggregateItemAdapter));
            AssertionConcern.AssertArgumentNotNull(externalReferenceAdapter, nameof(externalReferenceAdapter));
            AssertionConcern.AssertArgumentNotNull(neuronGraphQueryClient, nameof(neuronGraphQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.transaction = transaction;
            this.eventStore = eventStore;
            this.inMemoryEventStore = inMemoryEventStore;
            this.neuronGraphQueryClient = neuronGraphQueryClient;
            this.neuronAdapter = neuronAdapter;
            this.tagItemAdapter = tagItemAdapter;
            this.aggregateItemAdapter = aggregateItemAdapter;
            this.externalReferenceAdapter = externalReferenceAdapter;
            this.settingsService = settingsService;
        }

        public async Task<IEnumerable<Message>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, CancellationToken token = default)
        {
            if (!maxTimestamp.HasValue)
                maxTimestamp = DateTimeOffset.UtcNow;

            if (!pageSize.HasValue)
                pageSize = this.settingsService.PageSize;

            var result = await this.neuronGraphQueryClient.GetNeurons(
                this.settingsService.CortexGraphOutBaseUrl,
                new NeuronQuery()
                {
                    PostsynapticExternalReferenceUrl = new string[] { this.settingsService.InstantiatesMessageExternalReferenceUrl },
                    SortBy = SortByValue.NeuronCreationTimestamp,
                    SortOrder = SortOrderValue.Descending
                });

            return result.Neurons
                .Where(nr => DateTimeOffset.TryParse(nr.Creation.Timestamp, out DateTimeOffset currentCreationTimestamp) && currentCreationTimestamp < maxTimestamp)
                .Take(pageSize.Value)
                .Reverse()
                .Select(n => new Message()
                {
                    Id = Guid.Parse(n.Id),
                    Content = n.Tag,
                    Region = n.Region?.Tag,
                    RegionId = Guid.TryParse(n.Region?.Id, out Guid newRegionId) ? (Guid?) newRegionId : null,
                    Sender = n.Creation.Author.Tag,
                    SenderId = Guid.Parse(n.Creation.Author.Id),
                    CreationTimestamp = DateTimeOffset.TryParse(n.Creation.Timestamp, out DateTimeOffset creation) ? (DateTimeOffset?)creation : null,
                    LastModificationTimestamp = DateTimeOffset.TryParse(n.Creation.Timestamp, out DateTimeOffset lastModification) ? (DateTimeOffset?)lastModification : null
                });
        }

        public async Task Save(Message message, CancellationToken token = default)
        {
            int expectedVersion = await this.transaction.InvokeAdapter(
                    typeof(NeuronCreated).Assembly,
                    async (ev) => await this.neuronAdapter.CreateNeuron(
                        message.Id, 
                        message.SenderId),
                    0
                    );
            // assign tag value
            expectedVersion = await this.transaction.InvokeAdapter(
                typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly,
                async (ev) => await this.tagItemAdapter.ChangeTag(
                    message.Id,
                    message.Content,
                    message.SenderId,
                    ev
                ),
                expectedVersion
                );
            if (message.RegionId.HasValue)
            {
                // assign region value to id
                await this.transaction.InvokeAdapter(
                    typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly,
                    async (ev) => await this.aggregateItemAdapter.ChangeAggregate(
                        message.Id,
                        message.RegionId.ToString(),
                        message.SenderId,
                        ev
                    ),
                    expectedVersion
                    );
            }
        }
    }
}
