using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public interface IMessageReadRepository
    {
        Task<IEnumerable<Message>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, string userId, CancellationToken token = default(CancellationToken));
    }
}
