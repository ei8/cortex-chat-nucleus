using CQRSlite.Commands;
using ei8.Cortex.Chat.Nucleus.Application.Messages.Commands;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages
{
    public class MessageCommandHandlers : 
        ICancellableCommandHandler<CreateMessage>
    {
        private readonly ITransaction transaction;
        private readonly IMessageWriteRepository messageRepository;
        private readonly IRecipientWriteRepository recipientRepository;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;
        private readonly IIdentityService identityService;

        public MessageCommandHandlers(
            ITransaction transaction,
            IMessageWriteRepository messageRepository,
            IRecipientWriteRepository recipientRepository,
            IValidationClient validationClient, 
            ISettingsService settingsService,
            IIdentityService identityService
            )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(recipientRepository, nameof(recipientRepository));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(identityService, nameof(identityService));

            this.transaction = transaction;
            this.messageRepository = messageRepository;
            this.recipientRepository = recipientRepository;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
            this.identityService = identityService;
        }

        public async Task Handle(CreateMessage message, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            // validate
            var validationResult = await this.validationClient.CreateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                message.Id,
                message.RegionId,
                this.identityService.UserId,
                token
                );

            if (!validationResult.HasErrors)
            {
                var dMessage = new Message()
                {
                    Id = message.Id,
                    Content = message.Content,
                    RegionId = message.RegionId,
                    SenderId = validationResult.UserNeuronId,
                    ExternalReferenceUrl = message.ExternalReferenceUrl,
                    InstantiatesMessageTerminalId = Guid.NewGuid()
                };
                
                await this.transaction.BeginAsync(new Guid[] { dMessage.Id, dMessage.InstantiatesMessageTerminalId }, validationResult.UserNeuronId);

                await this.messageRepository.Save(dMessage);
                if (message.RecipientAvatarIds != null) 
                    await this.recipientRepository.SaveAll(
                        message.RecipientAvatarIds.Select(dri =>
                            new Recipient()
                            {
                                MessageId = message.Id,
                                Message = dMessage,
                                AvatarId = dri
                            }
                        )
                        );

                await this.transaction.CommitAsync();
            }
        }
    }
}

