﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages
{
    public interface IMessageQueryService
    {
        Task<IEnumerable<Common.MessageResult>> GetMessages(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Guid> avatarIds, string userId, CancellationToken token = default);
    }
}
