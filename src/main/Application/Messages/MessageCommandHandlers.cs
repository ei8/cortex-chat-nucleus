using CQRSlite.Commands;
using ei8.Cortex.Chat.Nucleus.Application.Messages.Commands;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.Coding.Persistence.Wrappers;
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
        private readonly IStringWrapperRepository stringWrapperRepository;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;
        private readonly IEnsembleTransactionData ensembleTransactionData;

        public MessageCommandHandlers(
            ITransaction transaction,
            IMessageWriteRepository messageRepository,
            IRecipientWriteRepository recipientRepository,
            IStringWrapperRepository stringWrapperRepository,
            IValidationClient validationClient, 
            ISettingsService settingsService,
            IEnsembleTransactionData ensembleTransactionData
        )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(recipientRepository, nameof(recipientRepository));
            AssertionConcern.AssertArgumentNotNull(stringWrapperRepository, nameof(stringWrapperRepository));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(ensembleTransactionData, nameof(ensembleTransactionData));

            this.transaction = transaction;
            this.messageRepository = messageRepository;
            this.recipientRepository = recipientRepository;
            this.stringWrapperRepository = stringWrapperRepository;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
            this.ensembleTransactionData = ensembleTransactionData;
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
                await this.transaction.BeginAsync(validationResult.UserNeuronId);

                var stringValue = new StringWrapper(message.Content);

                await this.stringWrapperRepository.Save(stringValue);

                var dMessage = new Message()
                {
                    Id = message.Id,
                    ContentId = this.ensembleTransactionData.GetReplacementIdIfExists(stringValue.Id),
                    RegionId = message.RegionId,
                    SenderId = validationResult.UserNeuronId,
                    ExternalReferenceUrl = message.ExternalReferenceUrl,
                    CreationTimestamp = DateTimeOffset.Now,
                    LastModificationTimestamp = DateTimeOffset.Now,
                    Version = message.ExpectedVersion
                };

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
                        ),
                        message.UserId
                    );

                await this.transaction.CommitAsync();
            }
        }
    }
}

