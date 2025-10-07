using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Library.Common;
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
    public class HttpMessageReadRepository : ReadRepositoryBase<Message>, IMessageReadRepository
    {
        /// <summary>
        /// Constructs a Message (read-only) Repository.
        /// </summary>
        /// <param name="networkRepository"></param>
        /// <param name="mirrorRepository"></param>
        /// <param name="neurULizer"></param>
        /// <param name="grannyService"></param>
        /// <param name="classInstanceNeuronsRetriever"></param>
        /// <param name="readWriteCache"></param>
        public HttpMessageReadRepository(
            INetworkRepository networkRepository,
            IMirrorRepository mirrorRepository,
            IneurULizer neurULizer,
            IGrannyService grannyService,
            IClassInstanceNeuronsRetriever classInstanceNeuronsRetriever,
            INetworkDictionary<CacheKey> readWriteCache
        ) : base(
            networkRepository,
            mirrorRepository,
            neurULizer,
            grannyService,
            classInstanceNeuronsRetriever,
            readWriteCache
        )
        {
        }

        /// <summary>
        /// Gets Messages using the specified IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="query"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Message>> GetByIds(
            IEnumerable<Guid> ids, 
            MessageQuery query = default,
            CancellationToken token = default
        ) => (await this.GetByIdsCore(
            ids,
            g => new NeuronQuery()
            {
                Postsynaptic = new[] { g.ToString() },
                Id = ids.Select(i => i.ToString()),
                SortBy = SortByValue.NeuronCreationTimestamp,
                SortOrder = SortOrderValue.Descending,
                Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                DirectionValues = DirectionValues.Outbound
            },
            false,
            token
        )).Take(query.PageSize.Value);

        /// <summary>
        /// Gets Messages using the specified parameters.
        /// </summary>
        /// <param name="query">Query parameters used during retrieval.</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Message>> GetByQuery(
            MessageQuery query = default,
            CancellationToken token = default
        )
        {
            GrannyResult instantiatesMessageResult = await ReadRepositoryBase<Message>.GetInstantiates(
                this.grannyService,
                this.mirrorRepository,
                token
            );

            return (await this.GetCore(
                async () => (await this.networkRepository.GetByQueryAsync(
                    new NeuronQuery()
                    {
                        Postsynaptic = new[] { instantiatesMessageResult.Granny.Neuron.Id.ToString() },
                        SortBy = SortByValue.NeuronCreationTimestamp,
                        SortOrder = SortOrderValue.Descending,
                        Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                        DirectionValues = DirectionValues.Outbound
                    },
                    false
                )).Network,
                token
            )).Take(query.PageSize.Value);
        }
    }
}
