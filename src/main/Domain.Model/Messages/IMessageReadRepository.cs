using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    public interface IMessageReadRepository
    {
        Task<IEnumerable<MessageResult>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Region> externalRegions, CancellationToken token = default);
    }
}
