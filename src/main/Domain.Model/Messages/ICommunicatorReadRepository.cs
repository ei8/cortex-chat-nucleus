using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Provides functionality for retrieving Communicators.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommunicatorReadRepository<T> where T : CommunicatorBase, new()
    {
        /// <summary>
        /// Retrieves CommunicatorBase instances using 
        /// the specified Avatar IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetByAvatarIds(
            IEnumerable<Guid> ids,
            CancellationToken token = default
        );

        /// <summary>
        /// Retrieve CommunicatorBase instances using 
        /// the specified Message IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetByMessageIds(
            IEnumerable<Guid> ids,
            CancellationToken token = default
        );
    }
}
