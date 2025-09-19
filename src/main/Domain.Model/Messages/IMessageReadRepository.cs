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
        /// Gets Messages using the specified parameters.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="maxTimestamp"></param>
        /// <param name="pageSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<Message>> GetByIds(
            IEnumerable<Guid> ids,
            DateTimeOffset? maxTimestamp = default, 
            int? pageSize = default,
            CancellationToken token = default
        );
    }
}
