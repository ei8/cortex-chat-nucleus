using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization;
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
    public class HttpAvatarReadRepository : IAvatarReadRepository
    {
        private readonly INetworkRepository networkRepository;
        private readonly IMirrorRepository mirrorRepository;
        private readonly IneurULizer neurULizer;
        private readonly IGrannyService grannyService;
        private readonly IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever;
        private readonly IIdInstanceNeuronsRetriever idInstanceNeuronsRetriever;

        public HttpAvatarReadRepository(
            INetworkRepository networkRepository,
            IMirrorRepository mirrorRepository,
            IneurULizer neurULizer,
            IGrannyService grannyService, 
            IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever,
            IIdInstanceNeuronsRetriever idInstanceNeuronsRetriever,
            IMirrorSet mirrorSet
        )
        {
            AssertionConcern.AssertArgumentNotNull(networkRepository, nameof(networkRepository));
            AssertionConcern.AssertArgumentNotNull(mirrorRepository, nameof(mirrorRepository));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));
            AssertionConcern.AssertArgumentNotNull(classInstanceNeuronsRetriever, nameof(classInstanceNeuronsRetriever));
            AssertionConcern.AssertArgumentNotNull(idInstanceNeuronsRetriever, nameof(idInstanceNeuronsRetriever));

            this.networkRepository = networkRepository;
            this.mirrorRepository = mirrorRepository;
            this.neurULizer = neurULizer;
            this.grannyService = grannyService;
            this.classInstanceNeuronsRetriever = classInstanceNeuronsRetriever;
            this.idInstanceNeuronsRetriever = idInstanceNeuronsRetriever;
        }

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
                instantiatesAvatarResult.Item1,
                $"'Instantiates^Avatar' is required to invoke {nameof(HttpAvatarReadRepository.GetAll)}"
            );
                       
            var queryResult = await this.networkRepository.GetByQueryAsync(
                new NeuronQuery()
                {
                    Postsynaptic = new string[] { instantiatesAvatarResult.Item2.Neuron.Id.ToString() },
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
            return (await this.neurULizer.DeneurULizeAsync<Avatar>(
                queryResult.Network, 
                this.classInstanceNeuronsRetriever,
                token
            )).Select(dm => dm.Result); 
        }

        public async Task<IEnumerable<Avatar>> GetByIds(IEnumerable<Guid> ids, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(ids, nameof(ids));
            AssertionConcern.AssertArgumentValid(i => i.Any(), ids, $"Specified value cannot be an empty array.", nameof(ids));

            var result = Enumerable.Empty<Avatar>();

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
            result = (await this.neurULizer.DeneurULizeAsync<Avatar>(
                queryResult.Network, 
                this.idInstanceNeuronsRetriever,
                token
            )).Select(nr => nr.Result);

            return result;
        }
    }
}
