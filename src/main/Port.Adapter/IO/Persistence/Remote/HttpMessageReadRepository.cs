using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.Cortex.Library.Common;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageReadRepository : IMessageReadRepository
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageQueryClient messageQueryClient;
        private readonly ISettingsService settingsService;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IEnumerable<ExternalReference> externalReferences;

        public HttpMessageReadRepository(
            // TODO:DEL remove serviceProvider
            IServiceProvider serviceProvider,
            IMessageQueryClient messageQueryClient,
            ISettingsService settingsService,
            IHttpClientFactory httpClientFactory, 
            IOptions<List<ExternalReference>> externalReferences
        )
        {
            AssertionConcern.AssertArgumentNotNull(serviceProvider, nameof(serviceProvider));
            AssertionConcern.AssertArgumentNotNull(messageQueryClient, nameof(messageQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(httpClientFactory, nameof(httpClientFactory));
            AssertionConcern.AssertArgumentNotNull(externalReferences, nameof(externalReferences));

            this.serviceProvider = serviceProvider;
            this.messageQueryClient = messageQueryClient;
            this.settingsService = settingsService;
            this.httpClientFactory = httpClientFactory;
            this.externalReferences = externalReferences.Value.ToArray();
        }

        public async Task<IEnumerable<MessageResult>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Avatar> avatars, string userId, CancellationToken token = default)
        {
            if (!maxTimestamp.HasValue)
                maxTimestamp = DateTimeOffset.UtcNow;

            if (!pageSize.HasValue)
                pageSize = this.settingsService.PageSize;

            var neurons = new QueryResult<Library.Common.Neuron>();

            var ensembleRepository = this.serviceProvider.GetRequiredService<IEnsembleRepository>();
            // use IInstantiatesClass type to find instance ids
            var primitives = await ensembleRepository.CreatePrimitives(userId);
            var instantiatesClass = await this.serviceProvider.GetRequiredService<
                    Coding.d23.neurULization.Processors.Readers.Deductive.IInstantiatesClassProcessor
                >().GetInstantiatesClass(
                // TODO: ideally should not use Id23neurULizerWriteOptions in GetAll,
                // but rather something like Id23neurULizerDeductiveReadOptions
                    new d23neurULizerWriteOptions(
                        primitives,
                        userId,
                        new WriteOptions(WriteMode.Update),
                        this.serviceProvider.GetRequiredService<Coding.d23.neurULization.Processors.Writers.IInstanceProcessor>(),
                        this.serviceProvider.GetRequiredService<IEnsembleRepository>(),
                        null
                    ),
                    await ensembleRepository.GetExternalReferenceAsync(
                        userId,
                        typeof(Message)
                    )
            );

            // TODO: specify maxTimestamp as a NeuronQuery parameter
            var ensemble = await ensembleRepository.GetByQueryAsync(
                userId,
                new NeuronQuery()
                {
                    Postsynaptic = new string[] { instantiatesClass.Neuron.Id.ToString() },
                    SortBy = SortByValue.NeuronCreationTimestamp,
                    SortOrder = SortOrderValue.Descending,
                    // from Instance granny to IValue-Instantiates
                    Depth = 12,
                    DirectionValues = DirectionValues.Outbound
                }
            );

            var dMessages = await new neurULizer().DeneurULizeAsync<Message>(
                ensemble,
                new d23neurULizerReadOptions(
                    primitives,
                    userId,
                    new ReadOptions(ReadMode.All),
                    instantiatesClass,
                    this.serviceProvider.GetRequiredService<Coding.d23.neurULization.Processors.Readers.Inductive.IInstanceProcessor>(),
                    this.serviceProvider.GetRequiredService<IEnsembleRepository>()
                )
            );

            var result = neurons.Items
                .Where(nr => DateTimeOffset.TryParse(nr.Creation.Timestamp, out DateTimeOffset currentCreationTimestamp) && currentCreationTimestamp <= maxTimestamp)
                .Take(pageSize.Value)
                .Reverse()
                .Select(n => new MessageResult()
                {
                    Message = new Message()
                    {
                        Id = Guid.Parse(n.Id),
                        Content = n.Tag,
                        RegionId = Guid.TryParse(n.Region?.Id, out Guid newRegionId) ? (Guid?)newRegionId : null,
                        SenderId = Guid.Parse(n.Creation.Author.Id),
                        CreationTimestamp = DateTimeOffset.TryParse(n.Creation.Timestamp, out DateTimeOffset creation) ? (DateTimeOffset?)creation : null,
                        UnifiedLastModificationTimestamp =
                        DateTimeOffset.TryParse(
                            n.UnifiedLastModification.Timestamp,
                            out DateTimeOffset unifiedLastModification) ?
                                (DateTimeOffset?)unifiedLastModification :
                                null
                    },
                    RegionTag = n.Region?.Tag,
                    SenderTag = n.Creation.Author.Tag,
                    IsCurrentUserCreationAuthor = n.Validation.IsCurrentUserCreationAuthor
                });

            if (avatars.Any())
            {
                var externalAvatarUrls = avatars
                    .Select(a =>
                        {
                            new Uri(a.ExternalReferenceUrl).ExtractParts(out string au, out string id);
                            return au;
                        })
                    .Distinct();

                var client = this.httpClientFactory.CreateClient("ignoreSSL");

                foreach (var eau in externalAvatarUrls)
                {
                    var authority = this.settingsService.Authorities.SingleOrDefault(au => au.Avatars.SingleOrDefault(av => av == eau) != null);

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
                            var remoteMessages = (await this.messageQueryClient.GetMessagesAsync(
                                eau + "/",
                                response.AccessToken, 
                                maxTimestamp,
                                pageSize,
                                token: token
                                )).Select(md => md.ToDomain());

                            result = result.Concat(remoteMessages);
                        }
                        catch (Exception ex)
                        {
                            var e = ex;
                        }
                    }
                    else
                    {
                        // TODO: log if authority for avatarurl was not found
                    }
                }
            }

            return result;
        }
    }
}
