using ei8.Cortex.Chat.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages
{
    public interface IMessageQueryService
    {
        Task<IEnumerable<MessageData>> GetMessages(DateTimeOffset? maxTimestamp, int? pageSize, string userId, CancellationToken token = default(CancellationToken));
    }
}
