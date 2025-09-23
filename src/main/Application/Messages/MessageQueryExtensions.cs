using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages
{
    public static class MessageQueryExtensions
    {
        /// <summary>
        /// Initializes MessageQuery.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="settingsService"></param>
        internal static MessageQuery Initialize(this MessageQuery query, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(query, nameof(query));

            if (!query.MaxTimestamp.HasValue)
                query.MaxTimestamp = DateTimeOffset.UtcNow;
            if (!query.PageSize.HasValue)
                query.PageSize = settingsService.PageSize;

            return query;
        }
    }
}
