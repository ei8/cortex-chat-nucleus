using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Coding;
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
    /// Represents an Avatar (read-only) Repository.
    /// </summary>
    public class HttpAvatarReadRepository : IAvatarReadRepository
    {
        private readonly INetworkRepository networkRepository;
        private readonly IMirrorRepository mirrorRepository;
        private readonly IneurULizer neurULizer;
        private readonly IGrannyService grannyService;
        private readonly IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever;
        private readonly IIdInstanceNeuronsRetriever idInstanceNeuronsRetriever;
        private readonly INetworkDictionary<CacheKey> readWriteCache;

        /// <summary>
        /// Constructs an Avatar (read-only) Repository.
        /// </summary>
        /// <param name="networkRepository"></param>
        /// <param name="mirrorRepository"></param>
        /// <param name="neurULizer"></param>
        /// <param name="grannyService"></param>
        /// <param name="classInstanceNeuronsRetriever"></param>
        /// <param name="idInstanceNeuronsRetriever"></param>
        /// <param name="readWriteCache"></param>
        public HttpAvatarReadRepository(
            INetworkRepository networkRepository,
            IMirrorRepository mirrorRepository,
            IneurULizer neurULizer,
            IGrannyService grannyService, 
            IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever,
            IIdInstanceNeuronsRetriever idInstanceNeuronsRetriever,
            INetworkDictionary<CacheKey> readWriteCache
        )
        {
            AssertionConcern.AssertArgumentNotNull(networkRepository, nameof(networkRepository));
            AssertionConcern.AssertArgumentNotNull(mirrorRepository, nameof(mirrorRepository));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));
            AssertionConcern.AssertArgumentNotNull(classInstanceNeuronsRetriever, nameof(classInstanceNeuronsRetriever));
            AssertionConcern.AssertArgumentNotNull(idInstanceNeuronsRetriever, nameof(idInstanceNeuronsRetriever));
            AssertionConcern.AssertArgumentNotNull(readWriteCache, nameof(readWriteCache));

            this.networkRepository = networkRepository;
            this.mirrorRepository = mirrorRepository;
            this.neurULizer = neurULizer;
            this.grannyService = grannyService;
            this.classInstanceNeuronsRetriever = classInstanceNeuronsRetriever;
            this.idInstanceNeuronsRetriever = idInstanceNeuronsRetriever;
            this.readWriteCache = readWriteCache;
        }

        /// <summary>
        /// Gets all Avatars.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Avatar>> GetAll(CancellationToken token = default)
        {
            var instantiatesAvatarResult = await this.grannyService.TryGetParseBuildPersistAsync(
                new InstantiatesClassGrannyInfo(
                    new Coding.d23.neurULization.Processors.Readers.Deductive.InstantiatesClassParameterSet(
                        await this.mirrorRepository.GetByKeyAsync(
                            typeof(Avatar)
                        )
                    )
                ),
                token
            );

            AssertionConcern.AssertStateTrue(
                instantiatesAvatarResult.Success,
                $"'Instantiates^Avatar' is required to invoke {nameof(HttpAvatarReadRepository.GetAll)}"
            );

            var queryResult = await this.networkRepository.GetByQueryAsync(
                new NeuronQuery()
                {
                    Postsynaptic = new string[] { instantiatesAvatarResult.Granny.Neuron.Id.ToString() },
                    SortBy = SortByValue.NeuronExternalReferenceUrl,
                    SortOrder = SortOrderValue.Ascending,
                    Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                    DirectionValues = DirectionValues.Outbound
                }
            );

            this.classInstanceNeuronsRetriever.Initialize(
                await this.mirrorRepository.GetByKeyAsync(
                    typeof(Avatar)
                )
            );

            return await this.neurULizer.DeneurULizeCacheAsync<Avatar>(
                queryResult.Network, 
                this.classInstanceNeuronsRetriever,
                this.readWriteCache[CacheKey.Read],
                token
            );
        }

        /// <summary>
        /// Gets Avatars using the specified IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Avatar>> GetByIds(IEnumerable<Guid> ids, CancellationToken token = default)
        {
            ids.ValidateIds();

            var queryResult = await this.networkRepository.GetByQueryAsync(
                new NeuronQuery()
                {
                    Id = ids.Select(i => i.ToString()),
                    SortBy = SortByValue.NeuronExternalReferenceUrl,
                    SortOrder = SortOrderValue.Ascending,
                    Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                    DirectionValues = DirectionValues.Outbound
                }
            );

            queryResult.Network.ValidateIds(ids);

            this.idInstanceNeuronsRetriever.Initialize(ids);
            var result = await this.neurULizer.DeneurULizeCacheAsync<Avatar>(
                queryResult.Network,
                this.idInstanceNeuronsRetriever,
                this.readWriteCache[CacheKey.Read],
                token
            );

            return result;
        }
    }
}
