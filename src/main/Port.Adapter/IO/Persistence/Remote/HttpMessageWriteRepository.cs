using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.Cortex.Coding.d23.neurULization.Processors.Writers;
using ei8.EventSourcing.Client;
using Microsoft.Extensions.DependencyInjection;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageWriteRepository : IMessageWriteRepository
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IEnsembleRepository ensembleRepository;
        private readonly IInstanceProcessor instanceProcessor;
        private readonly IDictionary<string, Ensemble> ensembleCache;

        public HttpMessageWriteRepository(
            // TODO:DEL
            IServiceProvider serviceProvider,
            IEnsembleRepository ensembleRepository,
            IInstanceProcessor instanceProcessor,
            IDictionary<string, Ensemble> ensembleCache
            )
        {
            AssertionConcern.AssertArgumentNotNull(ensembleRepository, nameof(ensembleRepository));
            AssertionConcern.AssertArgumentNotNull(instanceProcessor, nameof(instanceProcessor));
            AssertionConcern.AssertArgumentNotNull(ensembleCache, nameof(ensembleCache));

            this.serviceProvider = serviceProvider;
            this.ensembleRepository = ensembleRepository;
            this.instanceProcessor = instanceProcessor;
            this.ensembleCache = ensembleCache;
        }

        public async Task Save(Message message, string userId, CancellationToken token = default)
        {
            var me = await new neurULizer().neurULizeAsync(
                message, 
                new d23neurULizerWriteOptions(
                    await ensembleRepository.CreatePrimitives(userId),
                    userId,
                    new WriteOptions(
                        message.Version == 0 ?
                            WriteMode.Create :
                            WriteMode.Update
                    ),
                    this.instanceProcessor,
                    this.ensembleRepository,
                    this.ensembleCache
                )
            );

            await this.serviceProvider.GetRequiredService<ITransaction>()
                .SaveEnsembleAsync(this.serviceProvider, me, message.SenderId);
        }
    }
}
