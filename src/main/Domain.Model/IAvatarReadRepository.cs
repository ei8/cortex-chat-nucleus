using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public interface IAvatarReadRepository
    {
        Task<IEnumerable<Avatar>> GetAll(string userId, CancellationToken token = default);

        Task<IEnumerable<Avatar>> GetByIds(IEnumerable<Guid> ids, string userId, CancellationToken token = default);
    }
}
