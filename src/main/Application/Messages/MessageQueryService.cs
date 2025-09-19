using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.Persistence.Versioning;
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
        private readonly IAvatarReadRepository avatarRepository;
        private readonly IStringWrapperRepository stringWrapperRepository;
        private readonly ICreationReadRepository creationReadRepository;
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
        /// <param name="avatarRepository"></param>
        /// <param name="stringWrapperRepository"></param>
        /// <param name="creationReadRepository"></param>
        /// <param name="messageQueryClient"></param>
        /// <param name="validationClient"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="settingsService"></param>
        /// <param name="readWriteCache"></param>
        public MessageQueryService(
            IMessageReadRepository messageRepository, 
            ICommunicatorReadRepository<Sender> senderReadRepository,
            ICommunicatorReadRepository<Recipient> recipientReadRepository,
            IAvatarReadRepository avatarRepository,
            IStringWrapperRepository stringWrapperRepository,
            ICreationReadRepository creationReadRepository,
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
            AssertionConcern.AssertArgumentNotNull(avatarRepository, nameof(avatarRepository));
            AssertionConcern.AssertArgumentNotNull(stringWrapperRepository, nameof(stringWrapperRepository));
            AssertionConcern.AssertArgumentNotNull(creationReadRepository, nameof(creationReadRepository));
            AssertionConcern.AssertArgumentNotNull(messageQueryClient, nameof(messageQueryClient));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(httpClientFactory, nameof(httpClientFactory));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(readWriteCache, nameof(readWriteCache));

            this.messageRepository = messageRepository;
            this.senderReadRepository = senderReadRepository;
            this.recipientReadRepository = recipientReadRepository;
            this.avatarRepository = avatarRepository;
            this.stringWrapperRepository = stringWrapperRepository;
            this.creationReadRepository = creationReadRepository;
            this.messageQueryClient = messageQueryClient;
            this.validationClient = validationClient;
            this.httpClientFactory = httpClientFactory;
            this.settingsService = settingsService;
            this.readWriteCache = readWriteCache;
        }

        /// <summary>
        /// Gets Messages using the specified parameters.
        /// </summary>
        /// <param name="maxTimestamp"></param>
        /// <param name="pageSize"></param>
        /// <param name="avatarIds"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Common.MessageResult>> GetMessages(
            DateTimeOffset? maxTimestamp, 
            int? pageSize, 
            IEnumerable<Guid> avatarIds, 
            string userId, 
            CancellationToken token = default
        )
        {
            AssertionConcern.AssertArgumentNotNull(avatarIds, nameof(avatarIds));

            var result = new List<Common.MessageResult>();

            var senders = await this.senderReadRepository.GetByAvatarIds(avatarIds, token);

            if (senders.Any())
            {
                var localMessages = await this.messageRepository.GetByIds(
                    senders.Select(s => s.MessageId),
                    maxTimestamp,
                    pageSize,
                    token
                );

                if (localMessages.Any())
                {
                    IEnumerable<Message> pagedMessages = localMessages.Take(
                        pageSize.HasValue ?
                        pageSize.Value :
                        this.settingsService.PageSize
                    );

                    var messageIds = pagedMessages.Select(m => m.Id).Distinct();
                    var stringIds = pagedMessages.Select(m => m.ContentId).Distinct();

                    var validationResult = await this.validationClient.ReadNeurons(
                        this.settingsService.IdentityAccessOutBaseUrl + "/",
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

                    var contentStrings = await this.stringWrapperRepository.GetByIds(
                        stringIds,
                        token
                    );

                    var senderAvatars = await this.avatarRepository.GetByIds(
                        senders.Select(ls => ls.AvatarId).Distinct(),
                        token
                    );

                    var recipients = await this.recipientReadRepository.GetByMessageIds(
                        pagedMessages.Select(pm => pm.Id).Distinct(),
                        token
                    );

                    var recipientAvatars = await this.avatarRepository.GetByIds(
                        recipients.Select(ls => ls.AvatarId).Distinct(),
                        token
                    );

                    foreach (var pm in pagedMessages.Reverse())
                    {
                        result.Add(
                            await this.readWriteCache[CacheKey.Read].GetValidateNeuronAsync(
                                pm.Id,
                                async (n) =>
                                {
                                    var c = (await this.creationReadRepository.GetBySubjectId(pm.Id)).Single();
                                    var mSenders = senders.Where(ls => ls.MessageId == pm.Id).Select(lsm =>
                                        this.readWriteCache[CacheKey.Read].GetValidateNeuron(
                                            lsm.AvatarId,
                                            (an) =>
                                            {
                                                var senderAvatar = senderAvatars.Single(a => a.Id == lsm.AvatarId);
                                                return new CommunicatorInfo()
                                                {
                                                    Id = lsm.Id,
                                                    Avatar = new AvatarInfo()
                                                    {
                                                        Id = lsm.AvatarId,
                                                        Name = senderAvatar.Name,
                                                        MirrorUrl = an.MirrorUrl,
                                                        Url = an.Url
                                                    }
                                                };
                                            }
                                        )
                                    );
                                    var mRecipients = recipients.Where(ls => ls.MessageId == pm.Id).Select(lsm =>
                                        this.readWriteCache[CacheKey.Read].GetValidateNeuron(
                                            lsm.AvatarId,
                                            (an) =>
                                            {
                                                var recipientAvatar = recipientAvatars.Single(a => a.Id == lsm.AvatarId);
                                                return new CommunicatorInfo()
                                                {
                                                    Id = lsm.Id,
                                                    Avatar = new AvatarInfo()
                                                    {
                                                        Id = lsm.AvatarId,
                                                        Name = recipientAvatar.Name,
                                                        MirrorUrl = an.MirrorUrl,
                                                        Url = an.Url
                                                    }
                                                };
                                            }
                                        )
                                    );
                                    return new MessageResult()
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
                                        CreationTimestamp = c.Timestamp,
                                        UnifiedLastModificationTimestamp = n.UnifiedLastModificationTimestamp,
                                        IsCurrentUserCreationAuthor = mSenders.Any(ms => ms.Avatar.Id == validationResult.UserNeuronId)
                                    };
                                }
                            )
                        );
                    }
                }
            }

            var avatars = await this.avatarRepository.GetByIds(avatarIds);
            result.AddRange(
                await this.GetRemoteMessages(
                    avatars, 
                    pageSize, 
                    token
                )
            );

            return result;
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

                foreach (var am in avatars)
                {
                    var aur = string.Empty;
                    var aid = Guid.Empty;

                    var ammu = string.Empty;

                    this.readWriteCache[CacheKey.Read].GetValidateNeuron(am.Id, n => ammu = n.MirrorUrl);

                    AssertionConcern.AssertStateTrue(
                        !string.IsNullOrEmpty(ammu) && MirrorConfig.TryProcessUrl(ammu, out aur, out aid),
                        $"MirrorUrl of specified Avatar is invalid. ID: {am.Id.ToString()}"
                    );

                    var authority = this.settingsService.Authorities.SingleOrDefault(
                        au => au.Avatars.SingleOrDefault(av => av == aur) != null
                    );

                    AssertionConcern.AssertStateTrue(
                        authority != null,
                        $"Authority for Avatar '{am}' was not found."
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
                            $"Failed obtaining access token for Avatar '{am}': " +
                            $"{response.Error} - {response.ErrorDescription} - {response.HttpErrorReason} - {response.Exception}"
                        );

                        try
                        {
                            Trace.WriteLine($"Getting messages from '{am}' using avatarId '{aid}'.");

                            result.AddRange(await this.messageQueryClient.GetMessagesAsync(
                                aur + "/",
                                response.AccessToken,
                                null,
                                pageSize,
                                new[] { aid },
                                token
                            ));
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"An error occurred while sending a request to Avatar '{aur}'", ex);
                        }
                    }
                }
            }

            return result;
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
