using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    /// <summary>
    /// Represents a Message (read-only) repository.
    /// </summary>
    public class HttpMessageReadRepository : IMessageReadRepository
    {
        private readonly INetworkRepository networkRepository;
        private readonly IMirrorRepository mirrorRepository;
        private readonly ISettingsService settingsService;
        private readonly IneurULizer neurULizer;
        private readonly IGrannyService grannyService;
        private readonly IDictionary<string, IGranny> propertyAssociationCache;
        private readonly IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever;
        private readonly INetworkDictionary<CacheKey> readWriteCache;

        /// <summary>
        /// Constructs a Message Repository.
        /// </summary>
        /// <param name="networkRepository"></param>
        /// <param name="mirrorRepository"></param>
        /// <param name="settingsService"></param>
        /// <param name="neurULizer"></param>
        /// <param name="grannyService"></param>
        /// <param name="propertyAssociationCache"></param>
        /// <param name="classInstanceNeuronsRetriever"></param>
        /// <param name="readWriteCache"></param>
        public HttpMessageReadRepository(
            INetworkRepository networkRepository,
            IMirrorRepository mirrorRepository,
            ISettingsService settingsService,
            IneurULizer neurULizer,
            IGrannyService grannyService,
            IDictionary<string, IGranny> propertyAssociationCache,
            IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever,
            INetworkDictionary<CacheKey> readWriteCache
        )
        {
            AssertionConcern.AssertArgumentNotNull(networkRepository, nameof(networkRepository));
            AssertionConcern.AssertArgumentNotNull(mirrorRepository, nameof(mirrorRepository));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));
            AssertionConcern.AssertArgumentNotNull(propertyAssociationCache, nameof(propertyAssociationCache));
            AssertionConcern.AssertArgumentNotNull(classInstanceNeuronsRetriever, nameof(classInstanceNeuronsRetriever));
            AssertionConcern.AssertArgumentNotNull(readWriteCache, nameof(readWriteCache));

            this.networkRepository = networkRepository;
            this.mirrorRepository = mirrorRepository;
            this.settingsService = settingsService;
            this.neurULizer = neurULizer;
            this.grannyService = grannyService;
            this.propertyAssociationCache = propertyAssociationCache;
            this.classInstanceNeuronsRetriever = classInstanceNeuronsRetriever;
            this.readWriteCache = readWriteCache;
        }

        /// <summary>
        /// Gets Messages using the specified parameters.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="maxTimestamp"></param>
        /// <param name="pageSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Message>> GetByIds(
            IEnumerable<Guid> ids, 
            DateTimeOffset? maxTimestamp = null, 
            int? pageSize = null, 
            CancellationToken token = default
        )
        {
            ids.ValidateIds();

            if (!maxTimestamp.HasValue)
                maxTimestamp = DateTimeOffset.UtcNow;

            if (!pageSize.HasValue)
                pageSize = this.settingsService.PageSize;

            var result = Enumerable.Empty<Message>();
            var watch = System.Diagnostics.Stopwatch.StartNew();

            #region Get Instantiates
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
                instantiatesMessageResult.Success,
                $"'Instantiates^Message' is required to invoke {nameof(HttpMessageReadRepository.GetByIds)}"
            );
            #endregion

            #region Get Messages
            // Get Messages based on Senders
            if (ids.Any())
            {
                // TODO: specify maxTimestamp as a NeuronQuery parameter
                var queryResult = await networkRepository.GetByQueryAsync(
                    new NeuronQuery()
                    {
                        Postsynaptic = new[] {
                            instantiatesMessageResult.Granny.Neuron.Id.ToString(),
                        },
                        Id = ids.Select(i => i.ToString()),
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

                result = await this.neurULizer.DeneurULizeCacheAsync<Message>(
                    queryResult.Network,
                    this.classInstanceNeuronsRetriever,
                    this.readWriteCache[CacheKey.Read],
                    token
                );

                watch.Stop();
                System.Diagnostics.Debug.WriteLine($"Local GetAll took (secs): {watch.Elapsed.TotalSeconds}");
            }
            #endregion

            return result;
        }
    }
}
