using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageReadRepository : IMessageReadRepository
    {
        private readonly INetworkRepository networkRepository;
        private readonly IMirrorRepository mirrorRepository;
        private readonly ISettingsService settingsService;
        private readonly IneurULizer neurULizer;
        private readonly IGrannyService grannyService;
        private readonly IDictionary<string, IGranny> propertyAssociationCache;
        private readonly IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever;

        public HttpMessageReadRepository(
            INetworkRepository networkRepository,
            IMirrorRepository mirrorRepository,
            ISettingsService settingsService,
            IneurULizer neurULizer,
            IGrannyService grannyService,
            IDictionary<string, IGranny> propertyAssociationCache,
            IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever
        )
        {
            AssertionConcern.AssertArgumentNotNull(networkRepository, nameof(networkRepository));
            AssertionConcern.AssertArgumentNotNull(mirrorRepository, nameof(mirrorRepository));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));
            AssertionConcern.AssertArgumentNotNull(propertyAssociationCache, nameof(propertyAssociationCache));
            AssertionConcern.AssertArgumentNotNull(classInstanceNeuronsRetriever, nameof(classInstanceNeuronsRetriever));

            this.networkRepository = networkRepository;
            this.mirrorRepository = mirrorRepository;
            this.settingsService = settingsService;
            this.neurULizer = neurULizer;
            this.grannyService = grannyService;
            this.propertyAssociationCache = propertyAssociationCache;
            this.classInstanceNeuronsRetriever = classInstanceNeuronsRetriever;
        }

        public async Task<IEnumerable<Message>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Avatar> avatars, CancellationToken token = default)
        {
            if (!maxTimestamp.HasValue)
                maxTimestamp = DateTimeOffset.UtcNow;

            if (!pageSize.HasValue)
                pageSize = this.settingsService.PageSize;

            var result = Enumerable.Empty<Message>();
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var instantiatesMessageResult = await this.grannyService.TryGetParseBuildPersistAsync(
                new InstantiatesClassGrannyInfo(
                    new Coding.d23.neurULization.Processors.Readers.Deductive.InstantiatesClassParameterSet(
                        await this.mirrorRepository.GetByKeyAsync(
                            typeof(Message)
                        )
                    )
                ),
                token
            );

            AssertionConcern.AssertStateTrue(
                instantiatesMessageResult.Item1,
                $"'Instantiates^Message' is required to invoke {nameof(HttpMessageReadRepository.GetAll)}"
            );

            var hasSenderResult = await this.grannyService.TryGetPropertyValueAssociationFromCacheOrDb<Message>(
                this.mirrorRepository,
                this.networkRepository,
                nameof(Message.SenderId),
                avatars.Single().Id,
                this.propertyAssociationCache
            );

            if (hasSenderResult.Success)
            {
                // TODO: specify maxTimestamp as a NeuronQuery parameter
                var queryResult = await networkRepository.GetByQueryAsync(
                    new NeuronQuery()
                    {
                        Postsynaptic = new[] {
                            instantiatesMessageResult.Item2.Neuron.Id.ToString(),
                            // TODO: Add support for OR conditions in Cortex.Graph so that 
                            // messages with different senders can be retrieved
                            hasSenderResult.Granny.Neuron.Id.ToString()
                        },
                        SortBy = SortByValue.NeuronCreationTimestamp,
                        SortOrder = SortOrderValue.Descending,
                        Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                        DirectionValues = DirectionValues.Outbound
                    },
                    false
                );

                this.classInstanceNeuronsRetriever.Initialize(
                    await this.mirrorRepository.GetByKeyAsync(
                        typeof(Message)
                    )
                );
                result = (await this.neurULizer.DeneurULizeAsync<Message>(
                    queryResult.Network, 
                    this.classInstanceNeuronsRetriever,
                    token
                )).Select(dm => dm.Result);
                watch.Stop();
                System.Diagnostics.Debug.WriteLine($"Local GetAll took (secs): {watch.Elapsed.TotalSeconds}");
            }

            return result;
        }
    }
}
