using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    /// <summary>
    /// Provides functionality to query Avatars.
    /// </summary>
    public interface IAvatarQueryService
    {
        /// <summary>
        /// Get all Avatars using the specified userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<Common.AvatarInfo>> GetAvatars(string userId, CancellationToken token = default);

        /// <summary>
        /// Gets Avatars using the specified IDs and userId.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<Common.AvatarInfo>> GetAvatarsByIds(IEnumerable<Guid> ids, string userId, CancellationToken token = default);
    }
}
