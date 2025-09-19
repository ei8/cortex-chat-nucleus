using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages
{
    /// <summary>
    /// Provides functionality to query Messages.
    /// </summary>
    public interface IMessageQueryService
    {
        /// <summary>
        /// Gets Messages using the specified parameters.
        /// </summary>
        /// <param name="maxTimestamp"></param>
        /// <param name="pageSize"></param>
        /// <param name="avatarIds"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<Common.MessageResult>> GetMessages(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Guid> avatarIds, string userId, CancellationToken token = default);
    }
}
