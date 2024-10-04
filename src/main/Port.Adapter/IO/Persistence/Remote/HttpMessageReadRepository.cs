using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Library.Common;
using IdentityModel.Client;
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
        private readonly IEnsembleRepository ensembleRepository;
        private readonly IMessageQueryClient messageQueryClient;
        private readonly ISettingsService settingsService;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IneurULizer neurULizer;
        private readonly IGrannyService grannyService;

        public HttpMessageReadRepository(
            IEnsembleRepository ensembleRepository,
            IMessageQueryClient messageQueryClient,
            ISettingsService settingsService,
            IHttpClientFactory httpClientFactory,
            IneurULizer neurULizer,
            IGrannyService grannyService
        )
        {
            AssertionConcern.AssertArgumentNotNull(ensembleRepository, nameof(ensembleRepository));
            AssertionConcern.AssertArgumentNotNull(messageQueryClient, nameof(messageQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(httpClientFactory, nameof(httpClientFactory));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));

            this.ensembleRepository = ensembleRepository;
            this.messageQueryClient = messageQueryClient;
            this.settingsService = settingsService;
            this.httpClientFactory = httpClientFactory;
            this.neurULizer = neurULizer;
            this.grannyService = grannyService;
        }

        public async Task<IEnumerable<MessageResult>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Avatar> avatars, string userId, CancellationToken token = default)
        {
            if (!maxTimestamp.HasValue)
                maxTimestamp = DateTimeOffset.UtcNow;

            if (!pageSize.HasValue)
                pageSize = this.settingsService.PageSize;

            var neurons = new QueryResult<Library.Common.Neuron>();

            var instantiatesMessageResult = await this.grannyService.TryGetBuildPersistAsync(
                new InstantiatesClassGrannyInfo(
                    new Coding.d23.neurULization.Processors.Readers.Deductive.InstantiatesClassParameterSet(
                        await ensembleRepository.GetExternalReferenceAsync(
                            typeof(Message)
                        )
                    )
                ),
                token
            );

            AssertionConcern.AssertStateTrue(
                instantiatesMessageResult.Item1,
                $"'Instantiates^Message' is required to invoke {nameof(HttpMessageReadRepository.GetAll)}"
            );

            var hasSenderResult = await this.grannyService.TryGetGrannyAsync(
                await PropertyAssociationGrannyInfo.CreateById<Message>(
                    nameof(Message.SenderId),
                    this.ensembleRepository,
                    avatars.Single().Id
                )
            );

            // TODO: specify maxTimestamp as a NeuronQuery parameter
            var ensemble = await ensembleRepository.GetByQueryAsync(
                new NeuronQuery()
                {
                    Postsynaptic = new string[] { 
                        instantiatesMessageResult.Item2.Neuron.Id.ToString(),
                        hasSenderResult.Item2.Neuron.Id.ToString()
                    },
                    SortBy = SortByValue.NeuronCreationTimestamp,
                    SortOrder = SortOrderValue.Descending,
                    // from Instance granny to IValue-Instantiates
                    Depth = 12,
                    DirectionValues = DirectionValues.Outbound
                },
                userId
            );

            var dMessages = await this.neurULizer.DeneurULizeAsync<Message>(ensemble);

            var result = dMessages.Take(pageSize.Value)
                .Reverse()
                .Select(m => new MessageResult()
                {
                    Message = m,
                    RegionTag = m.RegionTag,
                    // TODO: should be based on SenderId
                    SenderTag = m.CreationAuthorTag,
                    // TODO: base on senderId
                    IsCurrentUserCreationAuthor = new Random().Next(100) < 50
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
