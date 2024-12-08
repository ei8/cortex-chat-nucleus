using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.Library.Common;
using IdentityModel.Client;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageReadRepository : IMessageReadRepository
    {
        private readonly IEnsembleRepository ensembleRepository;
        private readonly IExternalReferenceRepository externalReferenceRepository;
        private readonly IMessageQueryClient messageQueryClient;
        private readonly ISettingsService settingsService;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IneurULizer neurULizer;
        private readonly IGrannyService grannyService;
        private readonly IDictionary<string, IGranny> propertyAssociationCache;

        public HttpMessageReadRepository(
            IEnsembleRepository ensembleRepository,
            IExternalReferenceRepository externalReferenceRepository,
            IMessageQueryClient messageQueryClient,
            ISettingsService settingsService,
            IHttpClientFactory httpClientFactory,
            IneurULizer neurULizer,
            IGrannyService grannyService,
            IDictionary<string, IGranny> propertyAssociationCache
        )
        {
            AssertionConcern.AssertArgumentNotNull(ensembleRepository, nameof(ensembleRepository));
            AssertionConcern.AssertArgumentNotNull(externalReferenceRepository, nameof(externalReferenceRepository));
            AssertionConcern.AssertArgumentNotNull(messageQueryClient, nameof(messageQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(httpClientFactory, nameof(httpClientFactory));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));
            AssertionConcern.AssertArgumentNotNull(propertyAssociationCache, nameof(propertyAssociationCache));

            this.ensembleRepository = ensembleRepository;
            this.externalReferenceRepository = externalReferenceRepository;
            this.messageQueryClient = messageQueryClient;
            this.settingsService = settingsService;
            this.httpClientFactory = httpClientFactory;
            this.neurULizer = neurULizer;
            this.grannyService = grannyService;
            this.propertyAssociationCache = propertyAssociationCache;
        }

        public async Task<IEnumerable<MessageResult>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Avatar> avatars, string userId, CancellationToken token = default)
        {
            if (!maxTimestamp.HasValue)
                maxTimestamp = DateTimeOffset.UtcNow;

            if (!pageSize.HasValue)
                pageSize = this.settingsService.PageSize;

            var result = new List<MessageResult>();
            var localMessages = await this.GetLocalMessages(pageSize, avatars, userId, token);
            result.AddRange(localMessages);

            if (avatars.Any())
            {
                var externalAvatars = avatars
                    .Where(a => !string.IsNullOrWhiteSpace(a.ExternalReferenceUrl))
                    .Select(a =>
                    {
                        new Uri(a.ExternalReferenceUrl).ExtractParts(out string au, out string id);
                        return Tuple.Create(au, Guid.Parse(id));
                    })
                    .Distinct();

                var client = this.httpClientFactory.CreateClient("ignoreSSL");

                foreach (var eau in externalAvatars)
                {
                    var authority = this.settingsService.Authorities.SingleOrDefault(au => au.Avatars.SingleOrDefault(av => av == eau.Item1) != null);

                    AssertionConcern.AssertStateTrue(
                        authority != null,
                        $"Authority for Avatar '{eau.Item1}' was not found."
                    );

                    if (authority != null)
                    {
                        var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                        {
                            Address = authority.Address + "/connect/token",
                            ClientId = authority.ClientId,
                            ClientSecret = authority.ClientSecret
                        });

                        AssertionConcern.AssertStateFalse(
                            response.IsError,
                            $"Failed obtaining access token for Avatar '{eau.Item1}': " +
                            $"{response.Error} - {response.ErrorDescription} - {response.HttpErrorReason} - {response.Exception}"
                        );

                        try
                        {
                            Trace.WriteLine($"Getting messages from '{eau.Item1}' using avatarId '{eau.Item2}'.");

                            var remoteMessages = (await this.messageQueryClient.GetMessagesAsync(
                                eau.Item1 + "/",
                                response.AccessToken,
                                null,
                                pageSize,
                                new[] { eau.Item2 },
                                token
                                )).Select(md => md.ToDomain());

                            result = result.Concat(remoteMessages).ToList();
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"An error occurred while sending a request to Avatar '{eau.Item1}'", ex);
                        }
                    }
                }
            }

            return result;
        }

        private async Task<IEnumerable<MessageResult>> GetLocalMessages(int? pageSize, IEnumerable<Avatar> avatars, string userId, CancellationToken token)
        {
            var result = new List<MessageResult>();
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var instantiatesMessageResult = await this.grannyService.TryGetParseBuildPersistAsync(
                new InstantiatesClassGrannyInfo(
                    new Coding.d23.neurULization.Processors.Readers.Deductive.InstantiatesClassParameterSet(
                        await this.externalReferenceRepository.GetByKeyAsync(
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

            var hasSenderResult = await this.grannyService.TryGetPropertyAssociationFromCacheOrDb<Message>(
                this.externalReferenceRepository,
                nameof(Message.SenderId),
                avatars.Single().Id,
                this.propertyAssociationCache
            );

            if (hasSenderResult.Success)
            {
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
                    userId,
                    false
                );

                var dMessages = await this.neurULizer.DeneurULizeAsync<Message>(queryResult.Ensemble);
                watch.Stop();
                System.Diagnostics.Debug.WriteLine($"Local GetAll took (secs): {watch.Elapsed.TotalSeconds}");
                IEnumerable<Message> pagedMessages = dMessages.Take(pageSize.Value);

                var contentStringValues = await this.ensembleRepository.GetStringValues(
                    this.externalReferenceRepository,
                    this.grannyService,
                    queryResult.Ensemble,
                    pagedMessages.Select(m => m.ContentId).Distinct(),
                    userId
                );

                var senderNeuronsResult = await ensembleRepository.GetByQueryAsync(
                    new NeuronQuery() { Id = pagedMessages.Select(m => m.SenderId.ToString()).Distinct() }
                );

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
            }

            return result;
        }
    }
}
