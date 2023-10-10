using CQRSlite.Commands;
using ei8.Cortex.Chat.Nucleus.Application.Messages.Commands;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages
{
    public class MessageCommandHandlers : 
        ICancellableCommandHandler<CreateMessage> //,
        // TODO: ICancellableCommandHandler<ChangeNeuronTag>,
        //ICancellableCommandHandler<ChangeNeuronExternalReferenceUrl>,
        //ICancellableCommandHandler<DeactivateNeuron>,
        //ICancellableCommandHandler<ChangeNeuronRegionId>
    {
        private readonly ITransaction transaction;
        private readonly IMessageRepository messageRepository;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;

        public MessageCommandHandlers(
            ITransaction transaction,
            IMessageRepository messageRepository,
            IValidationClient validationClient, 
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.transaction = transaction;
            this.messageRepository = messageRepository;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
        }

        public async Task Handle(CreateMessage message, CancellationToken token = default(CancellationToken))
        {
            // TODO: transfer to domain.model.IMessageRepository
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            // validate
            var validationResult = await this.validationClient.CreateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                message.Id,
                message.RegionId,
                message.SenderId.ToString(),
                token);

            if (!validationResult.HasErrors)
            {
                //TODO: transfer all of this to Domain.Model, especially parse of Guid for region/aggregate
                await this.transaction.Begin(message.Id, validationResult.UserNeuronId);

                await this.messageRepository.Save(new Message()
                {
                    Id = message.Id,
                    Content = message.Content,
                    RegionId = message.RegionId,
                    SenderId = message.SenderId
                });

                await this.transaction.Commit();
            }
        }

        // TODO: public async Task Handle(ChangeNeuronTag message, CancellationToken token = default(CancellationToken))
        //{
        //    AssertionConcern.AssertArgumentNotNull(message, nameof(message));

        //    // validate
        //    var validationResult = await this.validationClient.UpdateNeuron(
        //        this.settingsService.IdentityAccessOutBaseUrl + "/",
        //        message.Id,
        //        message.UserId,
        //        token);

        //    if (!validationResult.HasErrors)
        //    {
        //        var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId, message.ExpectedVersion);
        //        await txn.InvokeAdapter(
        //            typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly,
        //            async (ev) => await this.tagItemAdapter.ChangeTag(
        //                message.Id,
        //                message.NewTag,
        //                validationResult.UserNeuronId,
        //                ev
        //            ));
        //        await txn.Commit();
        //    }
        //}

        //public async Task Handle(ChangeNeuronExternalReferenceUrl message, CancellationToken token = default(CancellationToken))
        //{
        //    AssertionConcern.AssertArgumentNotNull(message, nameof(message));

        //    // validate
        //    var validationResult = await this.validationClient.UpdateNeuron(
        //        this.settingsService.IdentityAccessOutBaseUrl + "/",
        //        message.Id,
        //        message.UserId,
        //        token);

        //    if (!validationResult.HasErrors)
        //    {
        //        var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId, message.ExpectedVersion);
        //        await txn.InvokeAdapter(
        //            typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly,
        //            async (ev) => await this.externalReferenceAdapter.ChangeUrl(
        //                message.Id,
        //                message.NewExternalReferenceUrl,
        //                validationResult.UserNeuronId,
        //                ev
        //            ));

        //        await txn.Commit();
        //    }
        //}

        //public async Task Handle(ChangeNeuronRegionId message, CancellationToken token = default(CancellationToken))
        //{
        //    AssertionConcern.AssertArgumentNotNull(message, nameof(message));

        //    // validate
        //    var validationResult = await this.validationClient.UpdateNeuron(
        //        this.settingsService.IdentityAccessOutBaseUrl + "/",
        //        message.Id,
        //        message.UserId,
        //        token);
        //    if (!validationResult.HasErrors)
        //    {
        //        var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId, message.ExpectedVersion);
        //        await txn.InvokeAdapter(
        //                typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly,
        //                async (ev) => await this.aggregateItemAdapter.ChangeAggregate(
        //                    message.Id,
        //                    message.NewRegionId,
        //                    validationResult.UserNeuronId,
        //                    ev
        //                ));

        //        await txn.Commit();
        //    }
        //} 

        //public async Task Handle(DeactivateNeuron message, CancellationToken token = default(CancellationToken))
        //{
        //    AssertionConcern.AssertArgumentNotNull(message, nameof(message));

        //    // validate
        //    var validationResult = await this.validationClient.UpdateNeuron(
        //        this.settingsService.IdentityAccessOutBaseUrl + "/",
        //        message.Id,
        //        message.UserId,
        //        token);

        //    if (!validationResult.HasErrors)
        //    {
        //        var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId, message.ExpectedVersion);
        //        await txn.InvokeAdapter(
        //            typeof(NeuronCreated).Assembly,
        //            async (ev) => await this.neuronAdapter.DeactivateNeuron(
        //                message.Id,
        //                validationResult.UserNeuronId,
        //                ev
        //            ));

        //        await txn.Commit();
        //    }
        //}
    }
}

