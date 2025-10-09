using ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    /// <summary>
    /// Provides functionality to retrieve Avatars.
    /// </summary>
    public interface IAvatarReadRepository
    {
        /// <summary>
        /// Gets all Avatars.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<Avatar>> GetAll(CancellationToken token = default);

        /// <summary>
        /// Gets Avatars using the specified IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<Avatar>> GetByIds(IEnumerable<Guid> ids, CancellationToken token = default);
    }
}
