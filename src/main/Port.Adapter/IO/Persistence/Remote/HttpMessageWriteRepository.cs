using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.In;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Library.Client.Out;
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
        private readonly ITransaction neuronTransaction;
        private readonly ITransaction terminalTransaction;
        private readonly IAuthoredEventStore neuronTransactionEventStore;
        private readonly IInMemoryAuthoredEventStore neuronTransactionInMemoryEventStore;
        private readonly INeuronAdapter neuronAdapter;
        private readonly ITerminalAdapter terminalAdapter;
        private readonly ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter;
        private readonly ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter;
        private readonly ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter;
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly IMessageClient messageClient;
        private readonly ISettingsService settingsService;
        private readonly IHttpClientFactory httpClientFactory;
        private static Guid? instantiatesMessageId;
        private static string currentAvatarUrl;

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
            INeuronQueryClient neuronQueryClient,
            IMessageClient messageClient,
            ISettingsService settingsService,
            IHttpClientFactory httpClientFactory
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
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(messageClient, nameof(messageClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(httpClientFactory, nameof(httpClientFactory));

            this.neuronTransaction = neuronTransaction;
            this.terminalTransaction = terminalTransaction;
            this.neuronTransactionEventStore = neuronTransactionEventStore;
            this.neuronTransactionInMemoryEventStore = neuronTransactionInMemoryEventStore;
            this.neuronAdapter = neuronAdapter;
            this.terminalAdapter = terminalAdapter;
            this.tagItemAdapter = tagItemAdapter;
            this.aggregateItemAdapter = aggregateItemAdapter;
            this.externalReferenceAdapter = externalReferenceAdapter;
            this.neuronQueryClient = neuronQueryClient;
            this.messageClient = messageClient;
            this.settingsService = settingsService;
            this.httpClientFactory=httpClientFactory;
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
               (await this.neuronTransactionEventStore.Get(await this.GetValidateInstantiatesMessageId(message.UserId), -1)).Concat(
               await this.neuronTransactionInMemoryEventStore.Get(message.Id, -1));

            await this.terminalTransaction.InvokeAdapter(
                typeof(TerminalCreated).Assembly,
                async (ev) => await this.terminalAdapter.CreateTerminal(
                    message.InstantiatesMessageTerminalId,
                    message.Id,
                    await this.GetValidateInstantiatesMessageId(message.UserId),
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
                expectedVersion = await this.neuronTransaction.InvokeAdapter(
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

            if (!string.IsNullOrWhiteSpace(message.ExternalReferenceUrl))
            {
                expectedVersion = await this.neuronTransaction.InvokeAdapter(
                    typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly,
                    async (ev) => await this.externalReferenceAdapter.ChangeUrl(
                        message.Id,
                        message.ExternalReferenceUrl,
                        message.SenderId,
                        ev
                    ),
                    expectedVersion
                    );
            }

            if (message.DestinationRegionIds.Any())
            {
                var client = this.httpClientFactory.CreateClient("ignoreSSL");

                foreach (var ri in message.DestinationRegionIds)
                {
                    // TODO: transfer region retrieval to a repository?
                    var exRefUrl = (
                            await this.neuronQueryClient.GetNeuronsInternal(
                                this.settingsService.CortexLibraryOutBaseUrl + "/",
                                new NeuronQuery()
                                {
                                    Id = new string[] { ri.ToString() },
                                    SortBy = SortByValue.NeuronCreationTimestamp,
                                    SortOrder = SortOrderValue.Descending
                                },
                                message.UserId
                            )
                        ).Items.SingleOrDefault()?.ExternalReferenceUrl;

                    if (!string.IsNullOrWhiteSpace (exRefUrl))
                    {
                        HttpMessageWriteRepository.ExtractParts(exRefUrl, out string avatarUrl, out string externalRegionId);

                        var authority = this.settingsService.Authorities.SingleOrDefault(au => au.Avatars.SingleOrDefault(av => av == avatarUrl) != null);

                        if (authority != null)
                        {
                            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                            {
                                Address = authority.Address + "/connect/token",
                                ClientId = authority.ClientId,
                                ClientSecret = authority.ClientSecret
                            });

                            try
                            {
                                await this.messageClient.CreateMessage(
                                    avatarUrl + "/",
                                    Guid.NewGuid().ToString(),
                                    message.Content,
                                    externalRegionId,
                                    $"{HttpMessageWriteRepository.currentAvatarUrl}/cortex/neurons/{message.Id}",
                                    null,
                                    response.AccessToken
                                    );
                            }
                            catch (Exception ex)
                            {
                                var e = ex;
                                // TODO: log any error while invoking destination
                            }
                        }
                        else
                        {
                            // TODO: log if authority for avatarurl was not found
                        }
                    }
                    else
                    {
                        // TODO: log exrefurl of local version of remote region was not found
                    }
                }
            }
        }

        private static void ExtractParts(string neuronUrl, out string avatarUrl, out string externalRegionId)
        {
            var match = Regex.Match(neuronUrl, "(?<AvatarUrl>.*)\\/cortex\\/neurons\\/(?<ExternalRegionId>.*)?");
            avatarUrl = match.Groups["AvatarUrl"].Value;
            externalRegionId = match.Groups["ExternalRegionId"].Value;
        }

        private async Task<Guid> GetValidateInstantiatesMessageId(string userId)
        {
            if (!HttpMessageWriteRepository.instantiatesMessageId.HasValue)
            {
                var instantiatesMessage = (
                            await this.neuronQueryClient.GetNeuronsInternal(
                                this.settingsService.CortexLibraryOutBaseUrl + "/",
                                new NeuronQuery()
                                {
                                    ExternalReferenceUrl = new string[] { this.settingsService.InstantiatesMessageExternalReferenceUrl },
                                    SortBy = SortByValue.NeuronCreationTimestamp,
                                    SortOrder = SortOrderValue.Descending
                                },
                                userId
                            )
                        ).Items.SingleOrDefault();

                if (instantiatesMessage != null)
                {
                    HttpMessageWriteRepository.ExtractParts(instantiatesMessage.Url, out string avatarUrl, out string externalRegionId);
                    HttpMessageWriteRepository.currentAvatarUrl = avatarUrl;
                    if (Guid.TryParse(instantiatesMessage.Id, out Guid result))
                    {
                        HttpMessageWriteRepository.instantiatesMessageId = result;
                    }
                }

                AssertionConcern.AssertStateTrue(HttpMessageWriteRepository.instantiatesMessageId.HasValue, "'Instantiates, Message' neuron was not found.");
            }

            return HttpMessageWriteRepository.instantiatesMessageId.Value;
        }
    }
}
