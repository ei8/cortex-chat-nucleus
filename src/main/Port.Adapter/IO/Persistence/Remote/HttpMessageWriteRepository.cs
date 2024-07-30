using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.EventSourcing.Client;
using Microsoft.Extensions.DependencyInjection;
using neurUL.Common.Domain.Model;
using System;
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
            var neuronRepository = this.serviceProvider.GetRequiredService<IEnsembleRepository>();
            var transaction = this.serviceProvider.GetRequiredService<ITransaction>();

            // required services
            var cores = await neuronRepository.GetExternalReferencesAsync(
                userId, 
                ExternalReferenceKey.DirectObject, 
                ExternalReferenceKey.Idea,
                ExternalReferenceKey.Instantiates,
                ExternalReferenceKey.Simple,
                ExternalReferenceKey.Subordination,
                ExternalReferenceKey.Coordination,
                ExternalReferenceKey.Unit,
                ExternalReferenceKey.Of,
                ExternalReferenceKey.Case,
                ExternalReferenceKey.NominalModifier,
                ExternalReferenceKey.Has
            );
            var coreSet = new PrimitiveSet()
            {
                DirectObject = cores[ExternalReferenceKey.DirectObject],
                Idea = cores[ExternalReferenceKey.Idea],
                Instantiates = cores[ExternalReferenceKey.Instantiates],
                Simple = cores[ExternalReferenceKey.Simple],
                Subordination = cores[ExternalReferenceKey.Subordination],
                Coordination = cores[ExternalReferenceKey.Coordination],
                Unit = cores[ExternalReferenceKey.Unit],
                Of = cores[ExternalReferenceKey.Of],
                Case = cores[ExternalReferenceKey.Case],
                NominalModifier = cores[ExternalReferenceKey.NominalModifier],
                Has = cores[ExternalReferenceKey.Has]
            };

            var nzer = new neurULizer();
            var me = await nzer.neurULizeAsync(message, new neurULizerOptions(
                this.serviceProvider,
                coreSet,
                userId,
                message.Version == 0 ?
                    WriteMode.Create :
                    WriteMode.Update
            ));

            await transaction.SaveEnsembleAsync(this.serviceProvider, me, message.SenderId);
        }
    }
}
