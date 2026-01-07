using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Mirrors;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Avatars
{
    /// <summary>
    /// Represents a query service for Avatars.
    /// </summary>
    public class AvatarQueryService : IAvatarQueryService
    {
        private readonly IAvatarReadRepository avatarRepository;
        private readonly INetworkDictionary<CacheKey> readWriteCache;

        public AvatarQueryService(
            IAvatarReadRepository avatarRepository,
            INetworkDictionary<CacheKey> readWriteCache
        )
        {
            AssertionConcern.AssertArgumentNotNull(avatarRepository, nameof(avatarRepository));
            AssertionConcern.AssertArgumentNotNull(readWriteCache, nameof(readWriteCache));

            this.avatarRepository = avatarRepository;
            this.readWriteCache = readWriteCache;
        }

        /// <summary>
        /// Get all Avatars using the specified userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Common.AvatarInfo>> GetAvatars(string userId, CancellationToken token = default)
        {
            // TODO:1 validate if user has access to neurons in result using validationClient
            return (await avatarRepository.GetAll(token))
                .Select(a =>
                    readWriteCache[CacheKey.Read].GetValidateNeuron(
                        a.Id,
                        n => new Common.AvatarInfo()
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Mirror = new MirrorInfo(n.MirrorUrl),
                            Url = n.Url
                        }
                    )
                );
        }

        /// <summary>
        /// Gets Avatars using the specified IDs and userId.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Common.AvatarInfo>> GetAvatarsByIds(IEnumerable<Guid> ids, string userId, CancellationToken token = default)
        {
            ids.ValidateIds();

            // TODO:1 validate if user has access to neurons in result using validationClient
            return (await avatarRepository.GetByIds(ids, token))
                .Select(a =>
                    readWriteCache[CacheKey.Read].GetValidateNeuron(
                        a.Id,
                        n => new Common.AvatarInfo()
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Mirror = new MirrorInfo(n.MirrorUrl),
                            Url = n.Url
                        }
                    )
                );
        }
    }
}
