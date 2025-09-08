using CQRSlite.Commands;
using ei8.Cortex.Chat.Nucleus.Application.Messages.Commands;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.Coding.Persistence.Versioning;
using ei8.Cortex.Coding.Persistence.Wrappers;
using ei8.Cortex.Coding.Versioning;
using ei8.Cortex.Coding.Wrappers;
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
        private readonly ICreationWriteRepository creationWriteRepository;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;
        private readonly INetworkTransactionData networkTransactionData;

        public MessageCommandHandlers(
            ITransaction transaction,
            IMessageWriteRepository messageRepository,
            IRecipientWriteRepository recipientRepository,
            IStringWrapperRepository stringWrapperRepository,
            ICreationWriteRepository creationWriteRepository,
            IValidationClient validationClient, 
            ISettingsService settingsService,
            INetworkTransactionData networkTransactionData
        )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(recipientRepository, nameof(recipientRepository));
            AssertionConcern.AssertArgumentNotNull(stringWrapperRepository, nameof(stringWrapperRepository));
            AssertionConcern.AssertArgumentNotNull(creationWriteRepository, nameof(creationWriteRepository));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(networkTransactionData, nameof(networkTransactionData));

            this.transaction = transaction;
            this.messageRepository = messageRepository;
            this.recipientRepository = recipientRepository;
            this.stringWrapperRepository = stringWrapperRepository;
            this.creationWriteRepository = creationWriteRepository;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
            this.networkTransactionData = networkTransactionData;
        }

        public async Task Handle(CreateMessage message, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            // validate
            // TODO:1 Revalidate later prior to creation of transient neurons?
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
                    ContentId = this.networkTransactionData.GetReplacementIdIfExists(stringValue.Id),
                    RegionId = message.RegionId,
                    SenderId = validationResult.UserNeuronId,
                    MirrorUrl = message.MirrorUrl,
                };

                await this.messageRepository.Save(dMessage);

                var messageCreation = new Creation()
                {
                    Id = Guid.NewGuid(),
                    SubjectId = dMessage.Id,
                    Timestamp = DateTimeOffset.Now
                };

                await this.creationWriteRepository.Save(messageCreation);

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
                // TODO:1 using validationClient, validate transient neurons against userId in network prior to commit?
                await this.transaction.CommitAsync();
            }
        }
    }
}

