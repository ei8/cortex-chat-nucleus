using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence.Wrappers;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.IdentityAccess.Common;
using IdentityModel.Client;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages
{
    /// <summary>
    /// Represents a query service for Messages.
    /// </summary>
    public class MessageQueryService : IMessageQueryService
    {
        private readonly IMessageReadRepository messageRepository;
        private readonly ICommunicatorReadRepository<Sender> senderReadRepository;
        private readonly ICommunicatorReadRepository<Recipient> recipientReadRepository;
        private readonly IAvatarReadRepository avatarReadRepository;
        private readonly IStringWrapperReadRepository stringWrapperReadRepository;
        private readonly IMessageQueryClient messageQueryClient;
        private readonly IValidationClient validationClient;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ISettingsService settingsService;
        private readonly INetworkDictionary<CacheKey> readWriteCache;

        /// <summary>
        /// Constructs a MessageQueryService.
        /// </summary>
        /// <param name="messageRepository"></param>
        /// <param name="senderReadRepository"></param>
        /// <param name="recipientReadRepository"></param>
        /// <param name="avatarReadRepository"></param>
        /// <param name="stringWrapperReadRepository"></param>
        /// <param name="messageQueryClient"></param>
        /// <param name="validationClient"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="settingsService"></param>
        /// <param name="readWriteCache"></param>
        public MessageQueryService(
            IMessageReadRepository messageRepository, 
            ICommunicatorReadRepository<Sender> senderReadRepository,
            ICommunicatorReadRepository<Recipient> recipientReadRepository,
            IAvatarReadRepository avatarReadRepository,
            IStringWrapperReadRepository stringWrapperReadRepository,
            IMessageQueryClient messageQueryClient,
            IValidationClient validationClient,
            IHttpClientFactory httpClientFactory,
            ISettingsService settingsService,
            INetworkDictionary<CacheKey> readWriteCache
        )
        {
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(senderReadRepository, nameof(senderReadRepository));
            AssertionConcern.AssertArgumentNotNull(recipientReadRepository, nameof(recipientReadRepository));
            AssertionConcern.AssertArgumentNotNull(avatarReadRepository, nameof(avatarReadRepository));
            AssertionConcern.AssertArgumentNotNull(stringWrapperReadRepository, nameof(stringWrapperReadRepository));
            AssertionConcern.AssertArgumentNotNull(messageQueryClient, nameof(messageQueryClient));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(httpClientFactory, nameof(httpClientFactory));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(readWriteCache, nameof(readWriteCache));

            this.messageRepository = messageRepository;
            this.senderReadRepository = senderReadRepository;
            this.recipientReadRepository = recipientReadRepository;
            this.avatarReadRepository = avatarReadRepository;
            this.stringWrapperReadRepository = stringWrapperReadRepository;
            this.messageQueryClient = messageQueryClient;
            this.validationClient = validationClient;
            this.httpClientFactory = httpClientFactory;
            this.settingsService = settingsService;
            this.readWriteCache = readWriteCache;
        }

        /// <summary>
        /// Gets Messages of Senders matching specified Avatar IDs.
        /// </summary>
        /// <param name="senderAvatarIds">Avatar IDs of Message Senders.</param>
        /// <param name="maxTimestamp"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Common.MessageResult>> GetMessages(
            IEnumerable<Guid> senderAvatarIds, 
            DateTimeOffset? maxTimestamp, 
            int? pageSize, 
            string userId, 
            CancellationToken token = default
        )
        {
            senderAvatarIds.ValidateIds();
            
            return await this.GetMessagesCore(
                async (q, t) =>
                {
                    IEnumerable<Message> localMessages = null;
                    var senders = await this.senderReadRepository.GetByAvatarIds(senderAvatarIds, t);
                    // get senders that match avatars
                    var matchedSenders = senders.Where(s => senderAvatarIds.Contains(s.AvatarId));
                    // if any matchedSenders were found
                    if (matchedSenders.Any())
                        //...return all messages of matched senders
                        localMessages = await this.messageRepository.GetByIds(
                            matchedSenders.Select(s => s.MessageId),
                            q,
                            t
                        );
                    else
                        // if no matched senders, return 0 messages
                        localMessages = Enumerable.Empty<Message>();

                    return (localMessages, senders);
                },
                async (t) =>
                {
                    var avatars = await this.avatarReadRepository.GetByIds(senderAvatarIds);
                    // remove owner avatar from 
                    var avatarsList = avatars.ToList();
                    var invalidAvatarIds = avatarsList.Where(
                        a => MessageQueryService.HasInvalidMirrorUrl(
                            a, 
                            this.readWriteCache[CacheKey.Read]
                        )
                    ).Select(a => a.Id);

                    AssertionConcern.AssertArgumentValid(
                        ids => !ids.Any(),
                        invalidAvatarIds,
                        $"The specifed Avatar IDs do not have valid mirror URLs: '{string.Join("', '", invalidAvatarIds)}'",
                        nameof(invalidAvatarIds)
                    );

                    return avatars;
                },
                maxTimestamp,
                pageSize,
                userId,
                token
            );
        }

        private static bool HasInvalidMirrorUrl(Avatar avatar, Network readCache)
        {
            AssertionConcern.AssertArgumentTrue(
                readCache.TryGetById(avatar.Id, out Neuron avatarNeuron),
                "Avatar neuron was not found in readCache."
            );
            return string.IsNullOrWhiteSpace(avatarNeuron.MirrorUrl);
        }

        /// <summary>
        /// Gets Messages using the specified parameters.
        /// </summary>
        /// <param name="maxTimestamp"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MessageResult>> GetMessages(
            DateTimeOffset? maxTimestamp, 
            int? pageSize, 
            string userId, 
            CancellationToken token = default
        )
        {
            return await this.GetMessagesCore(
                async (q, t) =>
                {
                    // if no avatars are specified, return all messages
                    var localMessages = await this.messageRepository.GetByQuery(q, t);
                    var senders = localMessages.Any() ?
                        await this.senderReadRepository.GetByMessageIds(localMessages.Select(m => m.Id).Distinct(), t) :
                        Enumerable.Empty<Sender>();
                    return (localMessages, senders);
                },
                async (t) =>
                {
                    var avatars = await this.avatarReadRepository.GetAll(token);
                    var avatarsList = avatars.ToList();
                    avatarsList.RemoveAll(a => MessageQueryService.HasInvalidMirrorUrl(a, this.readWriteCache[CacheKey.Read]));
                    return avatarsList.ToArray();
                },
                maxTimestamp, 
                pageSize, 
                userId, 
                token
            );
        }

        private async Task<IEnumerable<MessageResult>> GetMessagesCore(
            Func<MessageQuery, CancellationToken, Task<(IEnumerable<Message>, IEnumerable<Sender>)>> messagesRetriever,
            Func<CancellationToken, Task<IEnumerable<Avatar>>> remoteAvatarsRetriever,
            DateTimeOffset? maxTimestamp, 
            int? pageSize, 
            string userId, 
            CancellationToken token
        )
        {
            var query = new MessageQuery
            {
                MaxTimestamp = maxTimestamp,
                PageSize = pageSize,
            }.Initialize(this.settingsService);

            var result = new List<Common.MessageResult>();

            result.AddRange(
                await this.GetLocalMessages(
                    messagesRetriever,
                    query,
                    userId,
                    token
                )
            );

            var avatars = await remoteAvatarsRetriever(token);

            result.AddRange(
                await this.GetRemoteMessages(
                    avatars,
                    pageSize,
                    token
                )
            );

            return result;
        }

        private async Task<IEnumerable<MessageResult>> GetLocalMessages(
            Func<MessageQuery, CancellationToken, Task<(IEnumerable<Message>, IEnumerable<Sender>)>> messagesRetriever,
            MessageQuery query,
            string userId, 
            CancellationToken token
        )
        {
            var results = Enumerable.Empty<MessageResult>();

            (IEnumerable<Message> localMessages, IEnumerable<Sender> senders) = await messagesRetriever(query, token);
            
            if (localMessages.Any())
            {
                results = await MessageQueryService.CreateMessageResults(
                    userId,
                    senders,
                    localMessages,
                    this.settingsService,
                    this.stringWrapperReadRepository,
                    this.avatarReadRepository,
                    this.recipientReadRepository,
                    this.validationClient,
                    this.readWriteCache[CacheKey.Read],
                    token
                );
            }

            return results;
        }

        private static async Task<IEnumerable<MessageResult>> CreateMessageResults(
            string userId, 
            IEnumerable<Sender> senders, 
            IEnumerable<Message> localMessages, 
            ISettingsService settingsService,
            IStringWrapperReadRepository stringWrapperReadRepository,
            IAvatarReadRepository avatarReadRepository,
            ICommunicatorReadRepository<Recipient> recipientReadRepository,
            IValidationClient validationClient,
            Network readCache,
            CancellationToken token
        )
        {
            var results = new List<MessageResult>();

            var messageIds = localMessages.Select(m => m.Id).Distinct();
            var stringIds = localMessages.Select(m => m.ContentId).Distinct();

            var validationResult = await validationClient.ReadNeurons(
                settingsService.IdentityAccessOutBaseUrl + "/",
                MessageQueryService.Concatenate(
                    messageIds,
                    stringIds
                ).Distinct(),
                userId,
                token
            );

            AssertionConcern.AssertStateFalse(
                validationResult.HasErrors,
                $"Encountered error(s) while retrieving Neurons: " +
                $"Errors - '{string.Join("', '", validationResult.Errors.Select(e => e.Description))}'; " +
                $"Neuron Errors - '{MessageQueryService.ToString(validationResult)}'"
            );

            var contentStrings = await stringWrapperReadRepository.GetByIds(
                stringIds,
                token
            );

            var senderAvatars = await avatarReadRepository.GetByIds(
                senders.Select(ls => ls.AvatarId).Distinct(),
                token
            );

            var recipients = await recipientReadRepository.GetByMessageIds(
                localMessages.Select(pm => pm.Id).Distinct(),
                token
            );

            var recipientAvatars = await avatarReadRepository.GetByIds(
                recipients.Select(ls => ls.AvatarId).Distinct(),
                token
            );

            foreach (var pm in localMessages.Reverse())
            {
                results.Add(
                    await readCache.GetValidateNeuronAsync(
                        pm.Id,
                        (n) =>
                        {
                            var mSenders = MessageQueryService.GetCommunicatorInfos(
                                readCache,
                                senders,
                                senderAvatars,
                                pm
                            );
                            var mRecipients = MessageQueryService.GetCommunicatorInfos(
                                readCache, 
                                recipients, 
                                recipientAvatars, 
                                pm
                            );
                            return Task.FromResult(
                                new MessageResult()
                                {
                                    Id = pm.Id,
                                    Content = new StringInfo()
                                    {
                                        Id = pm.ContentId,
                                        Value = contentStrings.Single(csv => csv.Id == pm.ContentId).Tag
                                    },
                                    Region = n.RegionId.HasValue ? new NeuronInfo()
                                    {
                                        Id = n.RegionId.Value,
                                        Tag = n.RegionTag
                                    } : null,
                                    Senders = mSenders,
                                    Recipients = mRecipients,
                                    MirrorUrl = n.MirrorUrl,
                                    CreationTimestamp = pm.CreationTimestamp,
                                    UnifiedLastModificationTimestamp = n.UnifiedLastModificationTimestamp,
                                    IsCurrentUserCreationAuthor = mSenders.Any(ms => ms.Avatar.Id == validationResult.UserNeuronId)
                                }
                            );
                        }
                    )
                );
            }

            return results;
        }

        private static IEnumerable<CommunicatorInfo> GetCommunicatorInfos(
            Network readCache, 
            IEnumerable<CommunicatorBase> communicators, 
            IEnumerable<Avatar> communicatorAvatars, 
            Message pm
        )
        {
            return communicators
                .Where(c => c.MessageId == pm.Id)
                .Select(c =>
                    readCache.GetValidateNeuron(
                        c.AvatarId,
                        (an) =>
                        {
                            var communicatorAvatar = communicatorAvatars.Single(a => a.Id == c.AvatarId);
                            return new CommunicatorInfo()
                            {
                                Id = c.Id,
                                Avatar = new AvatarInfo()
                                {
                                    Id = c.AvatarId,
                                    Name = communicatorAvatar.Name,
                                    MirrorUrl = an.MirrorUrl,
                                    Url = an.Url
                                }
                            };
                        }
                    )
                );
        }

        private async Task<IEnumerable<MessageResult>> GetRemoteMessages(
            IEnumerable<Avatar> avatars, 
            int? pageSize, 
            CancellationToken token = default
        )
        {
            var result = new List<MessageResult>();

            if (avatars.Any())
            {
                var client = this.httpClientFactory.CreateClient("ignoreSSL");

                foreach (var avatar in avatars)
                {
                    Neuron avatarNeuron = null;
                    this.readWriteCache[CacheKey.Read].GetValidateNeuron(avatar.Id, n => avatarNeuron = n);

                    AssertionConcern.AssertStateTrue(
                        MessageQueryService.TryGetAuthorityByAvatarMirrorUrl(
                            avatarNeuron,
                            settingsService.Authorities,
                            out string remoteAvatarUrl,
                            out Guid remoteAvatarId,
                            out Authority authority
                        ),
                        $"Authority for Avatar '{avatar.Name}' was not found."
                    );

                    (bool tokenSuccess, TokenResponse tokenResponse) = await MessageQueryService.TryGetAvatarAccessToken(client, authority);
                    AssertionConcern.AssertStateTrue(
                        tokenSuccess,
                        $"Failed obtaining access token for Avatar '{avatar.Name}': " +
                        $"{tokenResponse.Error} - " +
                        $"{tokenResponse.ErrorDescription} - " +
                        $"{tokenResponse.HttpErrorReason} - " +
                        $"{tokenResponse.Exception}"
                    );

                    try
                    {
                        Trace.WriteLine($"Getting messages from '{avatar.Name}' using avatarId '{remoteAvatarId}'.");
                        result.AddRange(await this.messageQueryClient.GetMessagesAsync(
                            remoteAvatarUrl + "/",
                            tokenResponse.AccessToken,
                            null,
                            pageSize,
                            new[] { remoteAvatarId },
                            token
                        ));
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException($"An error occurred while sending a request to Avatar '{remoteAvatarUrl}'", ex);
                    }
                }
            }

            return result;
        }

        private static async Task<(bool, TokenResponse)> TryGetAvatarAccessToken(HttpClient client, Authority authority)
        {
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = authority.Address + "/connect/token",
                ClientId = authority.ClientId,
                ClientSecret = authority.ClientSecret
            });

            return (!response.IsError, response);
        }

        private static bool TryGetAuthorityByAvatarMirrorUrl(
            Neuron avatarNeuron, 
            IEnumerable<Authority> authorities,
            out string remoteAvatarUrl,
            out Guid remoteAvatarId,
            out Authority authority
        )
        {
            var tempRemoteAvatarUrl = string.Empty;
            Guid tempRemoteAvatarId = Guid.Empty;
            authority = null;

            AssertionConcern.AssertStateTrue(
                !string.IsNullOrEmpty(avatarNeuron.MirrorUrl) && 
                MirrorConfig.TryProcessUrl(avatarNeuron.MirrorUrl, out tempRemoteAvatarUrl, out tempRemoteAvatarId),
                $"MirrorUrl of specified Avatar is invalid. ID: {avatarNeuron.Id.ToString()}"
            );

            authority = authorities.SingleOrDefault(
                au => au.Avatars.SingleOrDefault(av => av == tempRemoteAvatarUrl) != null
            );

            remoteAvatarUrl = authority != null ? tempRemoteAvatarUrl : string.Empty;
            remoteAvatarId = authority !=  null ? tempRemoteAvatarId : Guid.Empty;

            return authority != null;
        }

        // TODO:1 Promote to appropriate library
        private static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
        {
            return lists.SelectMany(x => x);
        }

        // TODO:1 Promote to appropriate library
        private static string ToString(ActionValidationResult validationResult)
        {
            return string.Join(
                "', '", 
                validationResult.NeuronValidationResults.Select(
                    nvr => "Id: " + nvr.NeuronId + ", " +
                    "Errors: " + string.Join(
                        ", ",
                        nvr.Errors.Select(nvre => nvre.Description)
                    )
                )
            );
        }
    }
}
