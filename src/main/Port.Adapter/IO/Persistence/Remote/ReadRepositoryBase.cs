using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public abstract class ReadRepositoryBase<T>
        where T : class, new()
    {
        protected readonly INetworkRepository networkRepository;
        protected readonly IMirrorRepository mirrorRepository;
        protected readonly IneurULizer neurULizer;
        protected readonly IGrannyService grannyService;
        protected readonly IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever;
        protected readonly INetworkDictionary<CacheKey> readWriteCache;

        /// <summary>
        /// Constructs an Instance (read-only) Repository.
        /// </summary>
        /// <param name="networkRepository"></param>
        /// <param name="mirrorRepository"></param>
        /// <param name="neurULizer"></param>
        /// <param name="grannyService"></param>
        /// <param name="classInstanceNeuronsRetriever"></param>
        /// <param name="readWriteCache"></param>
        public ReadRepositoryBase(
            INetworkRepository networkRepository,
            IMirrorRepository mirrorRepository,
            IneurULizer neurULizer,
            IGrannyService grannyService,
            IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever,
            INetworkDictionary<CacheKey> readWriteCache
        )
        {
            AssertionConcern.AssertArgumentNotNull(networkRepository, nameof(networkRepository));
            AssertionConcern.AssertArgumentNotNull(mirrorRepository, nameof(mirrorRepository));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));
            AssertionConcern.AssertArgumentNotNull(classInstanceNeuronsRetriever, nameof(classInstanceNeuronsRetriever));
            AssertionConcern.AssertArgumentNotNull(readWriteCache, nameof(readWriteCache));

            this.networkRepository = networkRepository;
            this.mirrorRepository = mirrorRepository;
            this.neurULizer = neurULizer;
            this.grannyService = grannyService;
            this.classInstanceNeuronsRetriever = classInstanceNeuronsRetriever;
            this.readWriteCache = readWriteCache;
        }

        /// <summary>
        /// Gets all Instances.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetAll(CancellationToken token = default)
        {
            var instantiatesInstanceResult = await ReadRepositoryBase<T>.GetInstantiates(
                this.grannyService,
                this.mirrorRepository,
                token
            );

            var queryResult = await this.networkRepository.GetByQueryAsync(
                new NeuronQuery()
                {
                    Postsynaptic = new string[] { instantiatesInstanceResult.Granny.Neuron.Id.ToString() },
                    SortBy = SortByValue.NeuronExternalReferenceUrl,
                    SortOrder = SortOrderValue.Ascending,
                    Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                    DirectionValues = DirectionValues.Outbound
                }
            );

            this.classInstanceNeuronsRetriever.Initialize(
                await this.mirrorRepository.GetByKeyAsync(
                    typeof(T)
                )
            );

            return await this.neurULizer.DeneurULizeCacheAsync<T>(
                queryResult.Network,
                this.classInstanceNeuronsRetriever,
                this.readWriteCache[CacheKey.Read],
                token
            );
        }

        protected async Task<IEnumerable<T>> GetByIdsCore(
            IEnumerable<Guid> ids,
            Func<Guid, NeuronQuery> neuronQueryCreator,
            bool restrictQueryResultCount = true,
            CancellationToken token = default
        )
        {
            ids.ValidateIds();

            GrannyResult instantiatesInstanceResult = await ReadRepositoryBase<T>.GetInstantiates(
                this.grannyService,
                this.mirrorRepository,
                token
            );

            return await this.GetCore(
                async () => (await this.networkRepository.GetByQueryAsync(
                    neuronQueryCreator(instantiatesInstanceResult.Granny.Neuron.Id),
                    restrictQueryResultCount
                )).Network,
                token
            );
        }

        protected async Task<IEnumerable<T>> GetByPropertyAssociationValueIdsCore(
            IEnumerable<Guid> ids,
            IDictionary<string, IGranny> propertyAssociationCache,
            string propertyName,
            string methodName,
            bool restrictQueryResultCount = true,
            CancellationToken token = default
        )
        {
            ids.ValidateIds();

            var instantiatesCommunicatorResult = await ReadRepositoryBase<T>.GetInstantiates(
                this.grannyService,
                this.mirrorRepository,
                token
            );

            var communicatorHasPropertyAssociationValueIdsResults = await ReadRepositoryBase<T>.GetPropertyValueAssociationByValueIds(
                this.grannyService,
                this.mirrorRepository,
                this.networkRepository,
                propertyAssociationCache,
                ids,
                nameof(propertyName),
                nameof(methodName),
                token
            );

            return await this.GetCore(
                async () => {
                    Network cns = new Network();

                    // TODO:0 loop is not needed if NeuronQuery.Postsynaptic parameter accepts nested parameters
                    // eg. "x AND (y OR z)"
                    // Id = 1 AND (PropertyId = 2 or PropertyId = 3)
                    foreach (var chpavir in communicatorHasPropertyAssociationValueIdsResults)
                    {
                        if (chpavir.Success)
                        {
                            cns.AddReplaceItems(
                                (await this.networkRepository.GetByQueryAsync(
                                    new NeuronQuery()
                                    {
                                        Postsynaptic = new[] {
                                            instantiatesCommunicatorResult.Granny.Neuron.Id.ToString(),
                                            chpavir.Granny.Neuron.Id.ToString()
                                        },
                                        SortBy = SortByValue.NeuronCreationTimestamp,
                                        SortOrder = SortOrderValue.Descending,
                                        Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                                        DirectionValues = Library.Common.DirectionValues.Outbound
                                    },
                                    restrictQueryResultCount
                                )).Network
                            );
                        }
                    }

                    return cns;
                },
                token
            );
        }

        /// <summary>
        /// Gets Instances using the specified IDs and IInstanceNeuronsRetriever.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<T>> GetCore(
            Func<Task<Network>> networkRetriever,
            CancellationToken token = default
        )
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var network = await networkRetriever();

            this.classInstanceNeuronsRetriever.Initialize(
                await this.mirrorRepository.GetByKeyAsync(
                    typeof(T)
                )
            );

            var result = await this.neurULizer.DeneurULizeCacheAsync<T>(
                network,
                this.classInstanceNeuronsRetriever,
                this.readWriteCache[CacheKey.Read],
                token
            );

            watch.Stop();
            System.Diagnostics.Debug.WriteLine($"GetByIds took (secs): {watch.Elapsed.TotalSeconds}");

            return result;
        }

        protected static async Task<GrannyResult> GetInstantiates(
            IGrannyService grannyService, 
            IMirrorRepository mirrorRepository, 
            CancellationToken token
        )
        {
            var instantiatesInstanceResult = await grannyService.TryGetParseBuildPersistAsync(
                new InstantiatesClassGrannyInfo(
                    new Coding.d23.neurULization.Processors.Readers.Deductive.InstantiatesClassParameterSet(
                        await mirrorRepository.GetByKeyAsync(
                            typeof(T)
                        )
                    )
                ),
                token
            );

            AssertionConcern.AssertStateTrue(
                instantiatesInstanceResult.Success,
                $"'Instantiates^{typeof(T).Name}' is required to invoke {nameof(ReadRepositoryBase<T>.GetCore)}"
            );

            return instantiatesInstanceResult;
        }

        protected static async Task<IEnumerable<GrannyResult>> GetPropertyValueAssociationByValueIds(
            IGrannyService grannyService,
            IMirrorRepository mirrorRepository,
            INetworkRepository networkRepository,
            IDictionary<string, IGranny> propertyAssociationCache,
            IEnumerable<Guid> valueIds,
            string propertyName,
            string methodName,
            CancellationToken token
        )
        {
            var results = await grannyService.TryObtainPropertyValueAssociations<T>(
                mirrorRepository,
                networkRepository,
                propertyName,
                valueIds,
                propertyAssociationCache
            );

            return results;
        }
    }
}
