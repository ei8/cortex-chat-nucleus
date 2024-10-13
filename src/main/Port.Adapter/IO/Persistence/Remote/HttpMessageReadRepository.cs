using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
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

            var instantiatesMessageResult = await this.grannyService.TryGetParseBuildPersistAsync(
                new InstantiatesClassGrannyInfo(
                    new Coding.d23.neurULization.Processors.Readers.Deductive.InstantiatesClassParameterSet(
                        await this.ensembleRepository.GetExternalReferenceAsync(
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

            var hasSenderResult = (await this.grannyService.TryGetParseAsync(
                new[] {
                    await PropertyAssociationGrannyInfo.CreateById<Message>(
                        nameof(Message.SenderId),
                        this.ensembleRepository,
                        avatars.Single().Id
                    )
                }
            // TODO: should ideally be able to use multiple senders in query below
            )).Single();

            // TODO: specify maxTimestamp as a NeuronQuery parameter
            var queryResult = await ensembleRepository.GetByQueryAsync(
                new NeuronQuery()
                {
                    Postsynaptic = new string[] {
                        instantiatesMessageResult.Item2.Neuron.Id.ToString(),
                        // TODO: Add support for OR conditions in Cortex.Graph so that 
                        // messages with different senders can be retrieved
                        hasSenderResult.Granny.Neuron.Id.ToString()
                    },
                    SortBy = SortByValue.NeuronCreationTimestamp,
                    SortOrder = SortOrderValue.Descending,
                    Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                    DirectionValues = DirectionValues.Outbound
                },
                userId
            );

            var dMessages = await this.neurULizer.DeneurULizeAsync<Message>(queryResult.Ensemble);
            IEnumerable<Message> pagedMessages = dMessages.Take(pageSize.Value);

            var contentStringValues = await this.ensembleRepository.GetStringValues(
                this.grannyService,
                queryResult.Ensemble,
                pagedMessages.Select(m => m.ContentId).Distinct(),
                userId
            );

            var senderNeuronsResult = await ensembleRepository.GetByQueryAsync(
                new NeuronQuery() { Id = pagedMessages.Select(m => m.SenderId.ToString()).Distinct() }
            );

            var result = new List<MessageResult>();

            foreach (var pm in pagedMessages.Reverse())
            {
                var mr = new MessageResult()
                {
                    Message = pm,
                    ContentString = contentStringValues.GetTagById(pm.ContentId),
                    RegionTag = pm.RegionTag,
                    SenderTag = senderNeuronsResult.Ensemble.TryGetById(pm.SenderId, out Coding.Neuron sender) ?
                        sender.Tag :
                        "[Sender not found]",
                    IsCurrentUserSender = pm.SenderId == queryResult.UserNeuronId
                };
                result.Add(mr);
            }

            // TODO:
            //if (avatars.Any())
            //{
            //    var externalAvatarUrls = avatars
            //        .Select(a =>
            //        {
            //            new Uri(a.ExternalReferenceUrl).ExtractParts(out string au, out string id);
            //            return au;
            //        })
            //        .Distinct();

            //    var client = this.httpClientFactory.CreateClient("ignoreSSL");

            //    foreach (var eau in externalAvatarUrls)
            //    {
            //        var authority = this.settingsService.Authorities.SingleOrDefault(au => au.Avatars.SingleOrDefault(av => av == eau) != null);

            //        if (authority != null)
            //        {
            //            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            //            {
            //                Address = authority.Address + "/connect/token",
            //                ClientId = authority.ClientId,
            //                ClientSecret = authority.ClientSecret
            //            });

            //            try
            //            {
            //                var remoteMessages = (await this.messageQueryClient.GetMessagesAsync(
            //                    eau + "/",
            //                    response.AccessToken,
            //                    maxTimestamp,
            //                    pageSize,
            //                    token: token
            //                    )).Select(md => md.ToDomain());

            //                result = result.Concat(remoteMessages);
            //            }
            //            catch (Exception ex)
            //            {
            //                var e = ex;
            //            }
            //        }
            //        else
            //        {
            //            // TODO: log if authority for avatarurl was not found
            //        }
            //    }
            //}

            return result;
        }
    }
}
