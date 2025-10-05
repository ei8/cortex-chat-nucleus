using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    /// <summary>
    /// Represents a Communicator (read-only) Repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpCommunicatorReadRepository<T> : 
        ReadRepositoryBase<T>, 
        ICommunicatorReadRepository<T> where T : CommunicatorBase, new()
    {
        private readonly IDictionary<string, IGranny> propertyAssociationCache;

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
        ) : base(
            networkRepository,
            mirrorRepository,
            neurULizer,
            grannyService,
            classInstanceNeuronsRetriever,
            readWriteCache
        )
        {
            AssertionConcern.AssertArgumentNotNull(propertyAssociationCache, nameof(propertyAssociationCache));
        
            this.propertyAssociationCache = propertyAssociationCache;
        }

        /// <summary>
        /// Gets Communicators using the specified on AvatarIds.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetByAvatarIds(
            IEnumerable<Guid> ids, 
            CancellationToken token = default
        ) => await this.GetByPropertyAssociationValueIdsCore(
            ids,
            this.propertyAssociationCache,
            nameof(CommunicatorBase.AvatarId),
            nameof(HttpCommunicatorReadRepository<T>.GetByAvatarIds),
            false,
            token
        );

        /// <summary>
        /// Gets Communicators using the specified on MessageIds.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetByMessageIds(
            IEnumerable<Guid> ids, 
            CancellationToken token = default
        ) => await this.GetByPropertyAssociationValueIdsCore(
            ids,
            this.propertyAssociationCache,
            nameof(CommunicatorBase.MessageId),
            nameof(HttpCommunicatorReadRepository<T>.GetByMessageIds),
            false,
            token
        );
    }
}