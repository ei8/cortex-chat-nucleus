using ei8.Cortex.Coding.Mirrors;
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
        /// Gets Messages of Senders matching specified Avatar IDs.
        /// </summary>
        /// <param name="senderAvatarIds">Avatar IDs of Message Senders.</param>
        /// <param name="maxTimestamp"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<IMirrorImageSeries<Common.MessageResult>>> GetMessages(
            IEnumerable<Guid> senderAvatarIds, 
            DateTimeOffset? maxTimestamp, 
            int? pageSize, 
            bool includeRemote,
            string userId, 
            CancellationToken token = default
        );

        /// <summary>
        /// Gets Messages using the specified parameters.
        /// </summary>
        /// <param name="maxTimestamp"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<IMirrorImageSeries<Common.MessageResult>>> GetMessages(
            DateTimeOffset? maxTimestamp,
            int? pageSize,
            bool includeRemote,
            string userId,
            CancellationToken token = default
        );
    }
}
