using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars;
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
    /// Represents an Avatar (read-only) Repository.
    /// </summary>
    public class HttpAvatarReadRepository : ReadRepositoryBase<Avatar>, IAvatarReadRepository
    {
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
        /// Gets Avatars using the specified IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Avatar>> GetByIds(
            IEnumerable<Guid> ids, 
            CancellationToken token = default
        ) => await this.GetByIdsCore(
            ids,
            (g) => new NeuronQuery()
            {
                Postsynaptic = new[] { g.ToString() },
                Id = ids.Select(i => i.ToString()),
                SortBy = SortByValue.NeuronExternalReferenceUrl,
                SortOrder = SortOrderValue.Ascending,
                Depth = Coding.d23.neurULization.Constants.InstanceToValueInstantiatesClassDepth,
                DirectionValues = DirectionValues.Outbound
            },
            false
        );
    }
}
