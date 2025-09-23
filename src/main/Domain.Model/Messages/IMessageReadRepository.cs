using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Provides functionality for retrieving Messages.
    /// </summary>
    public interface IMessageReadRepository
    {
        /// <summary>
        /// Gets Messages using the specified IDs.
        /// </summary>
        /// <param name="ids">IDs of the Messages to be retrieved.</param>
        /// <param name="query">Query parameters used during retrieval.</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<Message>> GetByIds(
            IEnumerable<Guid> ids,
            MessageQuery query,
            CancellationToken token = default
        );

        /// <summary>
        /// Gets Messages using the specified parameters.
        /// </summary>
        /// <param name="query">Query parameters used during retrieval.</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<Message>> GetByQuery(
            MessageQuery query,
            CancellationToken token = default
        );
    }
}
