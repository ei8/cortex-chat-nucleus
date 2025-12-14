using CQRSlite.Commands;
using ei8.Cortex.Chat.Nucleus.Application.Avatars.Commands;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars;
using ei8.Cortex.Coding;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Avatars
{
    /// <summary>
    /// Represents a command handler for Avatars.
    /// </summary>
    public class AvatarCommandHandlers : 
        ICancellableCommandHandler<CreateAvatar>
    {
        private readonly ITransaction transaction;
        private readonly IAvatarWriteRepository avatarRepository;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;
        private readonly IWriteCacheService writeCacheService;

        /// <summary>
        /// Constructs a AvatarCommandHandler.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="avatarRepository"></param>
        /// <param name="validationClient"></param>
        /// <param name="settingsService"></param>
        /// <param name="writeCacheService"></param>
        public AvatarCommandHandlers(
            ITransaction transaction,
            IAvatarWriteRepository avatarRepository,
            IValidationClient validationClient,
            ISettingsService settingsService,
            IWriteCacheService writeCacheService
        )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(avatarRepository, nameof(avatarRepository));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(writeCacheService, nameof(writeCacheService));

            this.transaction = transaction;
            this.avatarRepository = avatarRepository;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
            this.writeCacheService = writeCacheService;
        }

        /// <summary>
        /// Handles the CreateAvatar command.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task Handle(CreateAvatar avatar, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(avatar, nameof(avatar));

            // validate
            // TODO:1 Revalidate later prior to creation of transient neurons?
            var validationResult = await this.validationClient.CreateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                avatar.Id,
                avatar.RegionId,
                avatar.UserId,
                token
                );

            if (!validationResult.HasErrors)
            {
                await this.transaction.BeginAsync(validationResult.UserNeuronId);
;
                await this.writeCacheService.SaveAsync(
                    new Avatar(
                        avatar.Id,
                        avatar.Name,
                        DateTimeOffset.Now 
                    ),
                    token,
                    (a) => Neuron.CreateTransient(
                        a.Id,
                        null,
                        avatar.MirrorUrl,
                        avatar.RegionId
                    ),
                    this.avatarRepository.Save
                );

                // TODO:1 using validationClient, validate transient neurons against userId in network prior to commit?
                await this.transaction.CommitAsync();
            }
        }
    }
}

