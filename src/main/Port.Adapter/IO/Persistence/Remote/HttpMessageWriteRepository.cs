using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.Cortex.Coding.d23.neurULization.Processors.Writers;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageWriteRepository : IMessageWriteRepository
    {
        private readonly IEnsembleRepository ensembleRepository;
        private readonly ITransaction transaction;
        private readonly IInstanceProcessor instanceProcessor;
        private readonly INeuronAdapter neuronAdapter;
        private readonly ITerminalAdapter terminalAdapter;
        private readonly Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter;
        private readonly Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter;
        private readonly Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceItemAdapter;
        private readonly IDictionary<string, Ensemble> ensembleCache;

        public HttpMessageWriteRepository(
            IEnsembleRepository ensembleRepository,
            ITransaction transaction,
            IInstanceProcessor instanceProcessor,
            neurUL.Cortex.Port.Adapter.In.InProcess.INeuronAdapter neuronAdapter,
            neurUL.Cortex.Port.Adapter.In.InProcess.ITerminalAdapter terminalAdapter,
            ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter,
            ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter,
            ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceItemAdapter,
            IDictionary<string, Ensemble> ensembleCache
            )
        {
            AssertionConcern.AssertArgumentNotNull(ensembleRepository, nameof(ensembleRepository));
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(instanceProcessor, nameof(instanceProcessor));
            AssertionConcern.AssertArgumentNotNull(neuronAdapter, nameof(neuronAdapter));
            AssertionConcern.AssertArgumentNotNull(terminalAdapter, nameof(terminalAdapter));
            AssertionConcern.AssertArgumentNotNull(tagItemAdapter, nameof(tagItemAdapter));
            AssertionConcern.AssertArgumentNotNull(aggregateItemAdapter, nameof(aggregateItemAdapter));
            AssertionConcern.AssertArgumentNotNull(externalReferenceItemAdapter, nameof(externalReferenceItemAdapter));
            AssertionConcern.AssertArgumentNotNull(ensembleCache, nameof(ensembleCache));

            this.ensembleRepository = ensembleRepository;
            this.transaction = transaction;
            this.instanceProcessor = instanceProcessor;
            this.neuronAdapter = neuronAdapter;
            this.terminalAdapter = terminalAdapter;
            this.tagItemAdapter = tagItemAdapter;
            this.aggregateItemAdapter = aggregateItemAdapter;
            this.externalReferenceItemAdapter = externalReferenceItemAdapter;
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

            await this.transaction.SaveEnsembleAsync(
                me, 
                message.SenderId,
                this.neuronAdapter,
                this.terminalAdapter,
                this.tagItemAdapter,
                this.aggregateItemAdapter,
                this.externalReferenceItemAdapter
            );
        }
    }
}
