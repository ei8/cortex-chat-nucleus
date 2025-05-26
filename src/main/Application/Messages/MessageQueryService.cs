using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
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
    public class MessageQueryService : IMessageQueryService
    {
        private readonly IMessageReadRepository messageRepository;
        private readonly IAvatarReadRepository avatarRepository;
        private readonly IStringWrapperRepository stringWrapperRepository;
        private readonly IMessageQueryClient messageQueryClient;
        private readonly IValidationClient validationClient;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ISettingsService settingsService;

        public MessageQueryService(
            IMessageReadRepository messageRepository, 
            IAvatarReadRepository avatarRepository,
            IStringWrapperRepository stringWrapperRepository,
            IMessageQueryClient messageQueryClient,
            IValidationClient validationClient,
            IHttpClientFactory httpClientFactory,
            ISettingsService settingsService
        )
        {
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(avatarRepository, nameof(avatarRepository));
            AssertionConcern.AssertArgumentNotNull(stringWrapperRepository, nameof(stringWrapperRepository));
            AssertionConcern.AssertArgumentNotNull(messageQueryClient, nameof(messageQueryClient));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(httpClientFactory, nameof(httpClientFactory));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.messageRepository = messageRepository;
            this.avatarRepository = avatarRepository;
            this.stringWrapperRepository = stringWrapperRepository;
            this.messageQueryClient = messageQueryClient;
            this.validationClient = validationClient;
            this.httpClientFactory = httpClientFactory;
            this.settingsService = settingsService;
        }

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
            var avatars = await this.avatarRepository.GetByIds(avatarIds);

            var localMessages = await this.messageRepository.GetAll(
                maxTimestamp,
                pageSize,
                avatars,
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
                var instanceAvatarIds = pagedMessages.Select(m => m.SenderId).Distinct();

                var validationResult = await this.validationClient.ReadNeurons(
                    this.settingsService.IdentityAccessOutBaseUrl + "/",
                    MessageQueryService.Concatenate(
                        messageIds,
                        stringIds,
                        instanceAvatarIds
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

                var contentStringValues = await this.stringWrapperRepository.GetByIds(
                    stringIds,
                    token
                );
                var senderAvatarValues = await
                    this.avatarRepository.GetByIds(
                    instanceAvatarIds,
                    token
                );
                
                foreach (var pm in pagedMessages.Reverse())
                {
                    result.Add(
                        new MessageResult()
                        {
                            Id = pm.Id,
                            ContentId = pm.ContentId,
                            ContentString = contentStringValues.Single(csv => csv.Id == pm.ContentId).Tag,
                            RegionTag = pm.RegionTag,
                            RegionId = pm.RegionId,
                            SenderId = pm.SenderId,
                            SenderTag = senderAvatarValues.Single(sav => sav.Id == pm.SenderId).Name,
                            MirrorUrl = pm.MirrorUrl,
                            CreationTimestamp = pm.CreationTimestamp,
                            UnifiedLastModificationTimestamp = pm.UnifiedLastModificationTimestamp,
                            IsCurrentUserCreationAuthor = pm.SenderId == validationResult.UserNeuronId
                        }
                    );
                }
            }

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

                    AssertionConcern.AssertStateTrue(
                        !string.IsNullOrEmpty(am.MirrorUrl) && MirrorConfig.TryProcessUrl(am.MirrorUrl, out aur, out aid),
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
