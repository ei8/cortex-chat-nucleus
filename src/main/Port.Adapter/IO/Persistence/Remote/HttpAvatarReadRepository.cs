using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Coding;
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
    public class HttpAvatarReadRepository : IAvatarReadRepository
    {
        private readonly INetworkRepository networkRepository;
        private readonly IExternalReferenceRepository externalReferenceRepository;
        private readonly IneurULizer neurULizer;
        private readonly IGrannyService grannyService;

        public HttpAvatarReadRepository(
            INetworkRepository networkRepository,
            IExternalReferenceRepository externalReferenceRepository,
            IneurULizer neurULizer,
            IGrannyService grannyService
        )
        {
            AssertionConcern.AssertArgumentNotNull(networkRepository, nameof(networkRepository));
            AssertionConcern.AssertArgumentNotNull(externalReferenceRepository, nameof(externalReferenceRepository));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));
            
            this.networkRepository = networkRepository;
            this.externalReferenceRepository = externalReferenceRepository;
            this.neurULizer = neurULizer;
            this.grannyService = grannyService;
        }

        public async Task<IEnumerable<Avatar>> GetAll(CancellationToken token = default)
        {
            var instantiatesAvatarResult = await this.grannyService.TryGetParseBuildPersistAsync(
                new InstantiatesClassGrannyInfo(
                    new Coding.d23.neurULization.Processors.Readers.Deductive.InstantiatesClassParameterSet(
                        await this.externalReferenceRepository.GetByKeyAsync(
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

            return await this.neurULizer.DeneurULizeAsync<Avatar>(queryResult.Network);
        }

        public async Task<IEnumerable<Avatar>> GetByIds(IEnumerable<Guid> ids, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(ids, nameof(ids));

            IEnumerable<Avatar> result = Array.Empty<Avatar>();

            if (ids.Any())
            {
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

                result = await this.neurULizer.DeneurULizeAsync<Avatar>(queryResult.Network);
            }

            return result;
        }
    }
}
