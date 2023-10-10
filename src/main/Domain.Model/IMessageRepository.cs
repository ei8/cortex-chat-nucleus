using ei8.Cortex.Chat.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, CancellationToken token = default(CancellationToken));

        Task Save(Message message, CancellationToken token = default(CancellationToken));
    }
}
