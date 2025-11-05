using CQRSlite.Commands;
using ei8.Cortex.Chat.Nucleus.Application.Messages.Commands;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.Model.Wrappers;
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
    /// <summary>
    /// Represents a command handler for Messages.
    /// </summary>
    public class MessageCommandHandlers : 
        ICancellableCommandHandler<CreateMessage>
    {
        private readonly ITransaction transaction;
        private readonly IMessageWriteRepository messageRepository;
        private readonly IStringWrapperWriteRepository stringWrapperWriteRepository;
        private readonly ICommunicatorWriteRepository<Sender> senderWriteRepository;
        private readonly ICommunicatorWriteRepository<Recipient> recipientWriteRepository;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;
        private readonly INetworkTransactionData networkTransactionData;
        private readonly IWriteCacheService writeCacheService;

        /// <summary>
        /// Constructs a MessageCommandHandler.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="messageRepository"></param>
        /// <param name="stringWrapperWriteRepository"></param>
        /// <param name="senderWriteRepository"></param>
        /// <param name="recipientWriteRepository"></param>
        /// <param name="validationClient"></param>
        /// <param name="settingsService"></param>
        /// <param name="networkTransactionData"></param>
        /// <param name="writeCacheService"></param>
        public MessageCommandHandlers(
            ITransaction transaction,
            IMessageWriteRepository messageRepository,
            IStringWrapperWriteRepository stringWrapperWriteRepository,
            ICommunicatorWriteRepository<Sender> senderWriteRepository,
            ICommunicatorWriteRepository<Recipient> recipientWriteRepository,
            IValidationClient validationClient,
            ISettingsService settingsService,
            INetworkTransactionData networkTransactionData,
            IWriteCacheService writeCacheService
        )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(stringWrapperWriteRepository, nameof(stringWrapperWriteRepository));
            AssertionConcern.AssertArgumentNotNull(senderWriteRepository, nameof(senderWriteRepository));
            AssertionConcern.AssertArgumentNotNull(recipientWriteRepository, nameof(recipientWriteRepository));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(networkTransactionData, nameof(networkTransactionData));
            AssertionConcern.AssertArgumentNotNull(writeCacheService, nameof(writeCacheService));

            this.transaction = transaction;
            this.messageRepository = messageRepository;
            this.stringWrapperWriteRepository = stringWrapperWriteRepository;
            this.senderWriteRepository = senderWriteRepository;
            this.recipientWriteRepository = recipientWriteRepository;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
            this.networkTransactionData = networkTransactionData;
            this.writeCacheService = writeCacheService;
        }

        /// <summary>
        /// Handles the CreateMessage command.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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

                #region Message
                var stringValue = new StringWrapper(message.Content);

                await this.stringWrapperWriteRepository.Save(stringValue);
                await this.writeCacheService.SaveAsync(
                    new Message()
                    {
                        Id = message.Id,
                        ContentId = this.networkTransactionData.GetReplacementIdIfExists(stringValue.Id),
                        CreationTimestamp = DateTimeOffset.Now
                    },
                    token,
                    (m) => Neuron.CreateTransient(
                        m.Id,
                        null,
                        message.MirrorUrl,
                        message.RegionId
                    ),
                    this.messageRepository.Save
                );
                #endregion

                #region Senders
                await this.writeCacheService.SaveAllAsync(
                    new Sender[]
                    {
                        new Sender()
                        {
                            Id = Guid.NewGuid(),
                            AvatarId = validationResult.UserNeuronId,
                            MessageId = message.Id
                        }
                    },
                    token,
                    (s) => Neuron.CreateTransient(
                        s.Id,
                        null,
                        null,
                        message.RegionId
                    ),
                    this.senderWriteRepository.SaveAll
                );
                #endregion

                #region Recipients
                await this.writeCacheService.SaveAllAsync(
                    message.RecipientAvatarIds.Select(rai => new Recipient()
                    {
                        Id = Guid.NewGuid(),
                        AvatarId = rai,
                        MessageId = message.Id
                    }
                    ),
                    token,
                    r => Neuron.CreateTransient(
                        r.Id,
                        null,
                        null,
                        message.RegionId
                    ),
                    this.recipientWriteRepository.SaveAll
                );
                #endregion
                
                // TODO:1 using validationClient, validate transient neurons against userId in network prior to commit?
                await this.transaction.CommitAsync();
            }
        }
    }
}

