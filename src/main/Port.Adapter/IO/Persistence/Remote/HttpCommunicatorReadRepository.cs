using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DirectionValues = ei8.Cortex.Library.Common.DirectionValues;
using NeuronQuery = ei8.Cortex.Library.Common.NeuronQuery;
using SortByValue = ei8.Cortex.Library.Common.SortByValue;
using SortOrderValue = ei8.Cortex.Library.Common.SortOrderValue;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    /// <summary>
    /// Represents a Communicator (read-only) Repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpCommunicatorReadRepository<T> : ICommunicatorReadRepository<T> where T : CommunicatorBase, new()
    {
        private readonly INetworkRepository networkRepository;
        private readonly IMirrorRepository mirrorRepository;
        private readonly IneurULizer neurULizer;
        private readonly IGrannyService grannyService;
        private readonly IDictionary<string, IGranny> propertyAssociationCache;
        private readonly IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever;
        private readonly INetworkDictionary<CacheKey> readWriteCache;

        /// <summary>
        /// Constructs a Communicator.
        /// </summary>
        /// <param name="networkRepository"></param>
        /// <param name="mirrorRepository"></param>
        /// <param name="neurULizer"></param>
        /// <param name="grannyService"></param>
        /// <param name="propertyAssociationCache"></param>
        /// <param name="classInstanceNeuronsRetriever"></param>
        /// <param name="readWriteCache"></param>
        public HttpCommunicatorReadRepository(
            INetworkRepository networkRepository,
            IMirrorRepository mirrorRepository,
            IneurULizer neurULizer,
            IGrannyService grannyService,
            IDictionary<string, IGranny> propertyAssociationCache,
            IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever,
            INetworkDictionary<CacheKey> readWriteCache
        )
        {
            AssertionConcern.AssertArgumentNotNull(networkRepository, nameof(networkRepository));
            AssertionConcern.AssertArgumentNotNull(mirrorRepository, nameof(mirrorRepository));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));
            AssertionConcern.AssertArgumentNotNull(propertyAssociationCache, nameof(propertyAssociationCache));
            AssertionConcern.AssertArgumentNotNull(classInstanceNeuronsRetriever, nameof(classInstanceNeuronsRetriever));
            AssertionConcern.AssertArgumentNotNull(readWriteCache, nameof(readWriteCache));

            this.networkRepository = networkRepository;
            this.mirrorRepository = mirrorRepository;
            this.neurULizer = neurULizer;
            this.grannyService = grannyService;
            this.propertyAssociationCache = propertyAssociationCache;
            this.classInstanceNeuronsRetriever = classInstanceNeuronsRetriever;
            this.readWriteCache = readWriteCache;
        }

        /// <summary>
        /// Gets Communicators using the specified on AvatarIds.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetByAvatarIds(IEnumerable<Guid> ids, CancellationToken token = default)
        {
            ids.ValidateIds();

            (
                GrannyResult instantiatesCommunicatorResult,
                IEnumerable<GrannyResult> communicatorHasAvatarIdsResults
            ) = await InitializeCommunicatorGet(
                ids, 
                nameof(CommunicatorBase.AvatarId), 
                nameof(HttpCommunicatorReadRepository<T>.GetByAvatarIds), 
                token
            );

            var results = Enumerable.Empty<T>();
            var communicatorNrs = Enumerable.Empty<neurULizationResult<T>>();
            Network communicatorsNetwork = new Network();
            // Get Communicators based on retrieved property associations above and instantiates^communicator
            if (communicatorHasAvatarIdsResults.Any() && communicatorHasAvatarIdsResults.All(chair => chair.Success))
            {
                // TODO:0 loop is not needed if NeuronQuery.Postsynaptic parameter accepts nested parameters
                // eg. "x AND (y OR z)"
                foreach (var chair in communicatorHasAvatarIdsResults)
                {
                    var queryResult = await networkRepository.GetByQueryAsync(
                        new NeuronQuery()
                        {
                            Postsynaptic = new[] {
                                instantiatesCommunicatorResult.Granny.Neuron.Id.ToString(),
                                chair.Granny.Neuron.Id.ToString()
                            },
                            SortBy = SortByValue.NeuronCreationTimestamp,
                            SortOrder = SortOrderValue.Descending,
                            Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                            DirectionValues = Library.Common.DirectionValues.Outbound
                        },
                        false
                    );
                   
                    communicatorsNetwork.AddReplaceItems(queryResult.Network);
                }

                this.classInstanceNeuronsRetriever.Initialize(
                    await this.mirrorRepository.GetByKeyAsync(
                        typeof(T)
                    )
                );

                results = await this.neurULizer.DeneurULizeCacheAsync<T>(
                    communicatorsNetwork,
                    this.classInstanceNeuronsRetriever,
                    this.readWriteCache[CacheKey.Read],
                    token
                );
            }

            return results;
        }

        /// <summary>
        /// Gets Communicators using the specified on MessageIds.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetByMessageIds(IEnumerable<Guid> ids, CancellationToken token = default)
        {
            ids.ValidateIds();

            (
                GrannyResult instantiatesCommunicatorResult, 
                IEnumerable<GrannyResult> communicatorHasAvatarIdsResults
            ) = await InitializeCommunicatorGet(
                ids, 
                nameof(CommunicatorBase.MessageId), 
                nameof(HttpCommunicatorReadRepository<T>.GetByMessageIds), 
                token
            );

            var results = Enumerable.Empty<T>();
            var communicatorNrs = Enumerable.Empty<neurULizationResult<T>>();
            Network communicatorsNetwork = new Network();
            // Get Communicators based on retrieved property associations above and instantiates^communicator
            if (communicatorHasAvatarIdsResults.Any() && communicatorHasAvatarIdsResults.All(shair => shair.Success))
            {
                // TODO:0 loop is not needed if NeuronQuery.Postsynaptic parameter accepts nested parameters
                // eg. "x AND (y OR z)"
                foreach (var chair in communicatorHasAvatarIdsResults)
                {
                    var queryResult = await networkRepository.GetByQueryAsync(
                        new NeuronQuery()
                        {
                            Postsynaptic = new[] {
                        instantiatesCommunicatorResult.Granny.Neuron.Id.ToString(),
                        chair.Granny.Neuron.Id.ToString()
                            },
                            SortBy = SortByValue.NeuronCreationTimestamp,
                            SortOrder = SortOrderValue.Descending,
                            Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                            DirectionValues = DirectionValues.Outbound
                        },
                        false
                    );

                    communicatorsNetwork.AddReplaceItems(queryResult.Network);
                }

                this.classInstanceNeuronsRetriever.Initialize(
                    await this.mirrorRepository.GetByKeyAsync(
                        typeof(T)
                    )
                );

                results = await this.neurULizer.DeneurULizeCacheAsync<T>(
                    communicatorsNetwork,
                    this.classInstanceNeuronsRetriever,
                    this.readWriteCache[CacheKey.Read],
                    token
                );
            }

            return results;
        }

        private async Task<(
            GrannyResult instantiatesCommunicatorResult, 
            IEnumerable<GrannyResult> communicatorHasAvatarIdsResults
        )> 
            InitializeCommunicatorGet(IEnumerable<Guid> ids, string propertyName, string methodName, CancellationToken token)
        {
            var instantiatesCommunicatorResult = await this.grannyService.TryGetParseBuildPersistAsync(
                new InstantiatesClassGrannyInfo(
                    new Coding.d23.neurULization.Processors.Readers.Deductive.InstantiatesClassParameterSet(
                        await this.mirrorRepository.GetByKeyAsync(
                            typeof(T)
                        )
                    )
                ),
                token
            );

            AssertionConcern.AssertStateTrue(
                instantiatesCommunicatorResult.Success,
                $"'Instantiates^{typeof(T).Name}' is required to invoke {methodName}"
            );

            // Get Communicator.HasAvatarIds based on specified avatarIds
            var communicatorHasAvatarIdsResults = await this.grannyService.TryObtainPropertyValueAssociations<T>(
                this.mirrorRepository,
                this.networkRepository,
                propertyName,
                ids,
                this.propertyAssociationCache
            );

            return (instantiatesCommunicatorResult, communicatorHasAvatarIdsResults);
        }
    }
}