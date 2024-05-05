using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.In;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Library.Common;
using ei8.EventSourcing.Client;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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

        public async Task Save(Message message, CancellationToken token = default)
        {
            var libraryService = this.serviceProvider.GetRequiredService<ILibraryService>();

            #region 'Hello World;Message' value
            var messageInstanceTagSuffix = ";Message";
            await this.serviceProvider.CreateInstanceAsync(
                message.Id,
                message.Content + messageInstanceTagSuffix,
                message.RegionId,
                message.ExternalReferenceUrl,
                await libraryService.GetNeuronId(ExternalReferenceId.InstantiatesMessage),
                message.SenderId
                );
            #endregion

            var cpvdi = new CreatePropertyValueDependencyIds(
                await libraryService.GetNeuronId(ExternalReferenceId.Unit),
                await libraryService.GetNeuronId(ExternalReferenceId.Subordination),
                await libraryService.GetNeuronId(ExternalReferenceId.Of_Case),
                await libraryService.GetNeuronId(ExternalReferenceId.NominalModifier),
                await libraryService.GetNeuronId(ExternalReferenceId.DirectObject),
                await libraryService.GetNeuronId(ExternalReferenceId.Has_Unit)
            );

            #region 'Content' property value
            var contentValueNeuronId = Guid.NewGuid();
            await this.serviceProvider.CreateInstanceAsync(
                contentValueNeuronId,
                message.Content,
                message.RegionId,
                null,
                await libraryService.GetNeuronId(ExternalReferenceId.InstantiatesIdea),
                message.SenderId
                );

            var contentPropNeuronId = await this.serviceProvider.CreatePropertyValueAsync(
               contentValueNeuronId,
               await libraryService.GetNeuronId(ExternalReferenceId.Message_MustHaveContent),
               message.SenderId,
               cpvdi
            );
            #endregion

            #region 'Author' property value
            var authorPropNeuronId = await this.serviceProvider.CreatePropertyValueAsync(
               message.SenderId,
               await libraryService.GetNeuronId(ExternalReferenceId.Message_MustHaveAuthor),
               message.SenderId,
               cpvdi
            );
            #endregion

            var nowTimestampValueNeuronId = Guid.NewGuid();
            await this.serviceProvider.CreateInstanceAsync(
                nowTimestampValueNeuronId,
                DateTimeOffset.UtcNow.ToString("o"),
                message.RegionId,
                null,
                await libraryService.GetNeuronId(ExternalReferenceId.InstantiatesDateTimeOffset),
                message.SenderId
                );

            #region 'CreationTimestamp' property value
            var creationPropNeuronId = await this.serviceProvider.CreatePropertyValueAsync(
               nowTimestampValueNeuronId,
               await libraryService.GetNeuronId(ExternalReferenceId.Message_MustHaveCreationTimestamp),
               message.SenderId,
               cpvdi
            );
            #endregion

            #region 'LastModificationTimestamp' property value
            var lastModificationPropNeuronId = await this.serviceProvider.CreatePropertyValueAsync(
               nowTimestampValueNeuronId,
               await libraryService.GetNeuronId(ExternalReferenceId.Message_MustHaveLastModificationTimestamp),
               message.SenderId,
               cpvdi
            );
            #endregion

            await this.serviceProvider.LinkInstancePropertyValuesAsync(
                message.Id,
                new Guid[]
                {
                    contentPropNeuronId,
                    authorPropNeuronId,
                    creationPropNeuronId,
                    lastModificationPropNeuronId
                },
                message.SenderId
                );
        }
    }
}
