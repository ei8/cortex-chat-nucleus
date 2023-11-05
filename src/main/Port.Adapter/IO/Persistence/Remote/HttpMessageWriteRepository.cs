using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
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

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageWriteRepository : IMessageWriteRepository
    {
        private readonly ITransaction neuronTransaction;
        private readonly ITransaction terminalTransaction;
        private readonly IAuthoredEventStore neuronTransactionEventStore;
        private readonly IInMemoryAuthoredEventStore neuronTransactionInMemoryEventStore;
        private readonly INeuronAdapter neuronAdapter;
        private readonly ITerminalAdapter terminalAdapter;
        private readonly ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter;
        private readonly ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter;
        private readonly ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter;
        private readonly INeuronGraphQueryClient neuronGraphQueryClient; 
        private readonly ISettingsService settingsService;
        private static Guid? instantiatesMessageId;

        public HttpMessageWriteRepository(
            ITransaction neuronTransaction,
            ITransaction terminalTransaction,
            IAuthoredEventStore neuronTransactionEventStore,
            IInMemoryAuthoredEventStore neuronTransactionInMemoryEventStore,
            INeuronAdapter neuronAdapter,
            ITerminalAdapter terminalAdapter,
            ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter,
            ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter,
            ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter,
            INeuronGraphQueryClient neuronGraphQueryClient,            
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(neuronTransaction, nameof(neuronTransaction));
            AssertionConcern.AssertArgumentNotNull(terminalTransaction, nameof(terminalTransaction));
            AssertionConcern.AssertArgumentNotNull(neuronTransactionEventStore, nameof(neuronTransactionEventStore));
            AssertionConcern.AssertArgumentNotNull(neuronTransactionInMemoryEventStore, nameof(neuronTransactionInMemoryEventStore));
            AssertionConcern.AssertArgumentNotNull(neuronAdapter, nameof(neuronAdapter));
            AssertionConcern.AssertArgumentNotNull(terminalAdapter, nameof(terminalAdapter));
            AssertionConcern.AssertArgumentNotNull(tagItemAdapter, nameof(tagItemAdapter));
            AssertionConcern.AssertArgumentNotNull(aggregateItemAdapter, nameof(aggregateItemAdapter));
            AssertionConcern.AssertArgumentNotNull(externalReferenceAdapter, nameof(externalReferenceAdapter));
            AssertionConcern.AssertArgumentNotNull(neuronGraphQueryClient, nameof(neuronGraphQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.neuronTransaction = neuronTransaction;
            this.terminalTransaction = terminalTransaction;
            this.neuronTransactionEventStore = neuronTransactionEventStore;
            this.neuronTransactionInMemoryEventStore = neuronTransactionInMemoryEventStore;
            this.neuronAdapter = neuronAdapter;
            this.terminalAdapter = terminalAdapter;
            this.tagItemAdapter = tagItemAdapter;
            this.aggregateItemAdapter = aggregateItemAdapter;
            this.externalReferenceAdapter = externalReferenceAdapter;
            this.neuronGraphQueryClient = neuronGraphQueryClient;
            this.settingsService = settingsService;
        }

        public async Task Save(Message message, CancellationToken token = default)
        {
            int expectedVersion = await this.neuronTransaction.InvokeAdapter(
                    typeof(NeuronCreated).Assembly,
                    async (ev) => await this.neuronAdapter.CreateNeuron(
                        message.Id, 
                        message.SenderId),
                    0
                    );

            // TODO: remove once terminalAdapter no longer requires otherAggregateEvents
            var otherAggregateEvents =
               (await this.neuronTransactionEventStore.Get(await this.GetValidateInstantiatesMessageId(), -1)).Concat(
               await this.neuronTransactionInMemoryEventStore.Get(message.Id, -1));

            await this.terminalTransaction.InvokeAdapter(
                typeof(TerminalCreated).Assembly,
                async (ev) => await this.terminalAdapter.CreateTerminal(
                    message.TerminalId,
                    message.Id,
                    await this.GetValidateInstantiatesMessageId(),
                    neurUL.Cortex.Common.NeurotransmitterEffect.Excite,
                    1f,
                    message.SenderId
                ),
                0,
                Transaction.ReplaceUnrecognizedEvents(otherAggregateEvents, typeof(NeuronCreated).Assembly)
                );

            // assign tag value
            expectedVersion = await this.neuronTransaction.InvokeAdapter(
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
                await this.neuronTransaction.InvokeAdapter(
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

        private async Task<Guid> GetValidateInstantiatesMessageId()
        {
            if (!HttpMessageWriteRepository.instantiatesMessageId.HasValue)
            {
                if (
                    Guid.TryParse(
                        (await this.neuronGraphQueryClient.GetNeurons(
                        this.settingsService.CortexGraphOutBaseUrl + "/",
                        new NeuronQuery()
                        {
                            ExternalReferenceUrl = new string[] { this.settingsService.InstantiatesMessageExternalReferenceUrl },
                            SortBy = SortByValue.NeuronCreationTimestamp,
                            SortOrder = SortOrderValue.Descending
                        })).Neurons.SingleOrDefault()?.Id, 
                        out Guid result
                    )
                )
                {
                    HttpMessageWriteRepository.instantiatesMessageId = result;
                }

                AssertionConcern.AssertStateTrue(HttpMessageWriteRepository.instantiatesMessageId.HasValue, "'Instantiates, Message' neuron was not found.");
            }

            return HttpMessageWriteRepository.instantiatesMessageId.Value;
        }
    }
}
