using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.In;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Library.Common;
using ei8.EventSourcing.Client;
using IdentityModel.Client;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageWriteRepository : IMessageWriteRepository
    {
        private readonly ITransaction transaction;
        private readonly INeuronAdapter neuronAdapter;
        private readonly ITerminalAdapter terminalAdapter;
        private readonly ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter;
        private readonly ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter;
        private readonly ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter;
        private readonly ILibraryService libraryService;

        public HttpMessageWriteRepository(
            ITransaction transaction,
            INeuronAdapter neuronAdapter,
            ITerminalAdapter terminalAdapter,
            ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter,
            ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter,
            ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter,
            ILibraryService libraryService
            )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(neuronAdapter, nameof(neuronAdapter));
            AssertionConcern.AssertArgumentNotNull(terminalAdapter, nameof(terminalAdapter));
            AssertionConcern.AssertArgumentNotNull(tagItemAdapter, nameof(tagItemAdapter));
            AssertionConcern.AssertArgumentNotNull(aggregateItemAdapter, nameof(aggregateItemAdapter));
            AssertionConcern.AssertArgumentNotNull(externalReferenceAdapter, nameof(externalReferenceAdapter));
            AssertionConcern.AssertArgumentNotNull(libraryService, nameof(libraryService));

            this.transaction = transaction;
            this.neuronAdapter = neuronAdapter;
            this.terminalAdapter = terminalAdapter;
            this.tagItemAdapter = tagItemAdapter;
            this.aggregateItemAdapter = aggregateItemAdapter;
            this.externalReferenceAdapter = externalReferenceAdapter;
            this.libraryService = libraryService;
        }

        public async Task Save(Message message, CancellationToken token = default)
        {
            #region Create Message neuron
            int expectedVersion = await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(NeuronCreated).Assembly.GetEventTypes(),
                    async (ev) => await this.neuronAdapter.CreateNeuron(
                        message.Id, 
                        message.SenderId)
                    );

            // assign tag value
            expectedVersion = await this.transaction.InvokeAdapterAsync(
                message.Id,
                typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly.GetEventTypes(),
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
                expectedVersion = await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly.GetEventTypes(),
                    async (ev) => await this.aggregateItemAdapter.ChangeAggregate(
                        message.Id,
                        message.RegionId.ToString(),
                        message.SenderId,
                        ev
                    ),
                    expectedVersion
                    );
            }

            if (!string.IsNullOrWhiteSpace(message.ExternalReferenceUrl))
            {
                expectedVersion = await this.transaction.InvokeAdapterAsync(
                    message.Id, 
                    typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly.GetEventTypes(),
                    async (ev) => await this.externalReferenceAdapter.ChangeUrl(
                        message.Id,
                        message.ExternalReferenceUrl,
                        message.SenderId,
                        ev
                    ),
                    expectedVersion
                    );
            }
            #endregion

            #region Create Instantiates, Message terminal
            await this.transaction.InvokeAdapterAsync(
                message.InstantiatesMessageTerminalId,
                typeof(TerminalCreated).Assembly.GetEventTypes(),
                async (ev) => await this.terminalAdapter.CreateTerminal(
                    message.InstantiatesMessageTerminalId,
                    message.Id,
                    await this.libraryService.GetId(TagValues.Message.Instantiates),
                    neurUL.Cortex.Common.NeurotransmitterEffect.Excite,
                    1f,
                    message.SenderId
                )
                );
            #endregion
        }
    }
}
