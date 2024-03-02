using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public interface IAvatarReadRepository
    {
        Task<IEnumerable<Avatar>> GetAll(CancellationToken token = default);

        Task<IEnumerable<Avatar>> GetByIds(IEnumerable<Guid> ids, CancellationToken token = default);
    }
}
