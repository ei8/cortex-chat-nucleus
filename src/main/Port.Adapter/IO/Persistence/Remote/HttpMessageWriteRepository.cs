using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.EnsembleServices;
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
            var instantiates = this.serviceProvider.GetRequiredService<IInstantiates>();
            var neuronRepository = this.serviceProvider.GetRequiredService<INeuronRepository>();
            var terminalService = this.serviceProvider.GetRequiredService<ITerminalRepository>();
            var transaction = this.serviceProvider.GetRequiredService<ITransaction>();

            var mn = await neuronRepository.GetExternalReferenceAsync(userId, typeof(Message).ToExternalReferenceKeyString());
            // TODO: transfer this inside neurULizer
            var n = await instantiates.ObtainAsync(new InstantiatesParameterSet(mn), neuronRepository, userId);

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
