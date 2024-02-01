using CQRSlite.Commands;
using ei8.Cortex.Chat.Nucleus.Application.Messages.Commands;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages
{
    public class MessageCommandHandlers : 
        ICancellableCommandHandler<CreateMessage>
    {
        private readonly ITransaction neuronTransaction;
        private readonly ITransaction terminalTransaction;
        private readonly IMessageWriteRepository messageRepository;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;

        public MessageCommandHandlers(
            ITransaction neuronTransaction,
            ITransaction terminalTransaction,
            IMessageWriteRepository messageRepository,
            IValidationClient validationClient, 
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(neuronTransaction, nameof(neuronTransaction));
            AssertionConcern.AssertArgumentNotNull(terminalTransaction, nameof(terminalTransaction));
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.neuronTransaction = neuronTransaction;
            this.terminalTransaction = terminalTransaction;
            this.messageRepository = messageRepository;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
        }

        public async Task Handle(CreateMessage message, CancellationToken token = default(CancellationToken))
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
                    UserId = message.UserId,
                    ExternalReferenceUrl = message.ExternalReferenceUrl,
                    DestinationRegionIds = message.DestinationRegionIds,
                    InstantiatesMessageTerminalId = Guid.NewGuid()
                };
                
                await this.neuronTransaction.Begin(dMessage.Id, validationResult.UserNeuronId);
                await this.terminalTransaction.Begin(dMessage.InstantiatesMessageTerminalId, validationResult.UserNeuronId);

                await this.messageRepository.Save(dMessage);

                await this.neuronTransaction.Commit();
                await this.terminalTransaction.Commit();
            }
        }
    }
}

