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
        private readonly ITransaction neuronTransaction;
        private readonly ITransaction terminalTransaction;
        private readonly IMessageWriteRepository messageRepository;
        private readonly IDestinationWriteRepository destinationRepository;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;
        private readonly IIdentityService identityService;

        public MessageCommandHandlers(
            ITransaction neuronTransaction,
            ITransaction terminalTransaction,
            IMessageWriteRepository messageRepository,
            IDestinationWriteRepository destinationRepository,
            IValidationClient validationClient, 
            ISettingsService settingsService,
            IIdentityService identityService
            )
        {
            AssertionConcern.AssertArgumentNotNull(neuronTransaction, nameof(neuronTransaction));
            AssertionConcern.AssertArgumentNotNull(terminalTransaction, nameof(terminalTransaction));
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(destinationRepository, nameof(destinationRepository));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(identityService, nameof(identityService));

            this.neuronTransaction = neuronTransaction;
            this.terminalTransaction = terminalTransaction;
            this.messageRepository = messageRepository;
            this.destinationRepository = destinationRepository;
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
                
                await this.neuronTransaction.Begin(dMessage.Id, validationResult.UserNeuronId);
                await this.terminalTransaction.Begin(dMessage.InstantiatesMessageTerminalId, validationResult.UserNeuronId);

                await this.messageRepository.Save(dMessage);
                await this.destinationRepository.SaveAll(
                    message.DestinationRegionIds.Select(dri =>
                        new Destination()
                        {
                            MessageId = message.Id,
                            Message = dMessage,
                            RegionId = dri
                        }
                    )
                    );

                await this.neuronTransaction.Commit();
                await this.terminalTransaction.Commit();
            }
        }
    }
}

