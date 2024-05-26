using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex;
using ei8.EventSourcing.Client;
using Microsoft.Extensions.DependencyInjection;
using neurUL.Common.Domain.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageWriteRepository : IMessageWriteRepository
    {
        private readonly IServiceProvider serviceProvider;
        
        public HttpMessageWriteRepository(IServiceProvider serviceProvider)
        {
            AssertionConcern.AssertArgumentNotNull(serviceProvider, nameof(serviceProvider));
            
            this.serviceProvider = serviceProvider;
        }

        public async Task Save(Message message, string userId, CancellationToken token = default)
        {
            var neuronService = this.serviceProvider.GetRequiredService<INeuronService>();
            var terminalService = this.serviceProvider.GetRequiredService<ITerminalService>();
            var transaction = this.serviceProvider.GetRequiredService<ITransaction>();

            #region Temp Code  - For Creating an 'Instantiates^[Class]~do' ensemble
            //var ers = await neuronService.GetExternalReferences(
            //    userId,
            //    ExternalReferenceKey.DirectObject,
            //    typeof(Domain.Model.Messages.Message),
            //    ExternalReferenceKey.InstantiatesMessage,
            //    ExternalReferenceKey.Subordination,
            //    ExternalReferenceKey.Instantiates_Unit
            //    );

            //// TODO: use NeuronQueryClient to determine whether 'Message~do' already exists in DB
            //// if not yet in DB 
            //// ... use to create neuron in memory
            //// var message_do = neuronService.CreateTransient();
            //// message_do.Tag = "Message~do";

            //// ... use to create based on NeuronData
            //var message_do = new NeuronData()
            //    {
            //        Id = Guid.Parse("3748be0d-94aa-40bf-b209-cb359194886c"),
            //        Tag = "Message~do"
            //    }.ToEnsemble();

            //var links1 = await terminalService.GetOrCreateTerminalsIfNotExistsAsync(
            //    message_do,
            //    userId,
            //    ers[ExternalReferenceKey.DirectObject],
            //    ers[typeof(Domain.Model.Messages.Message)]
            //    );

            //var instantiates_message_do = ers[ExternalReferenceKey.InstantiatesMessage];

            //var links2 = await terminalService.GetOrCreateTerminalsIfNotExistsAsync(
            //    instantiates_message_do,
            //    userId,
            //    ers[ExternalReferenceKey.Subordination],
            //    ers[ExternalReferenceKey.Instantiates_Unit],
            //    message_do
            //    );

            //var sd = instantiates_message_do.ToEnsembleData();

            ////await transaction.SaveEnsembleDataAsync(this.serviceProvider, sd, message.SenderId);
            #endregion

            var n = await neuronService.GetInstantiatesClassNeurons(userId, typeof(Message).ToExternalReferenceKeyString());

            //#region 'Hello World;Message' value
            //var messageInstanceTagSuffix = ";Message";
            //await this.serviceProvider.CreateInstanceAsync(
            //    message.Id,
            //    message.Content + messageInstanceTagSuffix,
            //    message.RegionId,
            //    message.ExternalReferenceUrl,
            //    (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.InstantiatesMessage, userId)).Id,
            //    message.SenderId
            //    );
            //#endregion

            //var cpvdi = new CreatePropertyValueDependencyIds(
            //    (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.Unit, userId)).Id,
            //    (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.Subordination, userId)).Id,
            //    (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.Of_Case, userId)).Id,
            //    (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.NominalModifier, userId)).Id,
            //    (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.DirectObject, userId)).Id,
            //    (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.Has_Unit, userId)).Id
            //);

            //#region 'Content' property value
            //var contentValueNeuronId = Guid.NewGuid();
            //await this.serviceProvider.CreateInstanceAsync(
            //    contentValueNeuronId,
            //    message.Content,
            //    message.RegionId,
            //    null,
            //    (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.InstantiatesIdea, userId)).Id,
            //    message.SenderId
            //    );

            //var contentPropNeuronId = await this.serviceProvider.CreatePropertyValueAsync(
            //   contentValueNeuronId,
            //   (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.Message_MustHaveContent, userId)).Id,
            //   message.SenderId,
            //   cpvdi
            //);
            //#endregion

            //#region 'Author' property value
            //var authorPropNeuronId = await this.serviceProvider.CreatePropertyValueAsync(
            //   message.SenderId,
            //   (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.Message_MustHaveAuthor, userId)).Id,
            //   message.SenderId,
            //   cpvdi
            //);
            //#endregion

            //var nowTimestampValueNeuronId = Guid.NewGuid();
            //await this.serviceProvider.CreateInstanceAsync(
            //    nowTimestampValueNeuronId,
            //    DateTimeOffset.UtcNow.ToString("o"),
            //    message.RegionId,
            //    null,
            //    (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.InstantiatesDateTimeOffset, userId)).Id,
            //    message.SenderId
            //    );

            //#region 'CreationTimestamp' property value
            //var creationPropNeuronId = await this.serviceProvider.CreatePropertyValueAsync(
            //   nowTimestampValueNeuronId,
            //   (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.Message_MustHaveCreationTimestamp, userId)).Id,
            //   message.SenderId,
            //   cpvdi
            //);
            //#endregion

            //#region 'LastModificationTimestamp' property value
            //var lastModificationPropNeuronId = await this.serviceProvider.CreatePropertyValueAsync(
            //   nowTimestampValueNeuronId,
            //   (await neuronService.GetOrCreateIfNotExistsAsync(ExternalReferenceKey.Message_MustHaveLastModificationTimestamp, userId)).Id,
            //   message.SenderId,
            //   cpvdi
            //);
            //#endregion

            //await this.serviceProvider.LinkInstancePropertyValuesAsync(
            //    message.Id,
            //    new Guid[]
            //    {
            //        contentPropNeuronId,
            //        authorPropNeuronId,
            //        creationPropNeuronId,
            //        lastModificationPropNeuronId
            //    },
            //    message.SenderId
            //    );
        }
    }
}
