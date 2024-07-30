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

        public MessageCommandHandlers(
            ITransaction transaction,
            IMessageWriteRepository messageRepository,
            IRecipientWriteRepository recipientRepository,
            IValidationClient validationClient, 
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(recipientRepository, nameof(recipientRepository));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            
            this.transaction = transaction;
            this.messageRepository = messageRepository;
            this.recipientRepository = recipientRepository;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
        }

        public async Task Handle(CreateMessage message, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            // validate
            var validationResult = await this.validationClient.CreateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                message.Id,
                message.RegionId,
                message.UserId,
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
                    CreationTimestamp = DateTimeOffset.Now,
                    LastModificationTimestamp = DateTimeOffset.Now,
                    Version = message.ExpectedVersion
                };

                await this.transaction.BeginAsync(validationResult.UserNeuronId);

                await this.messageRepository.Save(dMessage, message.UserId);
                if (message.RecipientAvatarIds != null) 
                    await this.recipientRepository.SaveAll(
                        message.RecipientAvatarIds.Select(dri =>
                            new Recipient()
                            {
                                MessageId = message.Id,
                                Message = dMessage,
                                AvatarId = dri
                            }
                        ),
                        message.UserId
                    );

                await this.transaction.CommitAsync();
            }
        }
    }
}

