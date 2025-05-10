using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public interface IAvatarQueryService
    {
        Task<IEnumerable<Common.AvatarResult>> GetAvatars(string userId, CancellationToken token = default);

        Task<IEnumerable<Common.AvatarResult>> GetAvatarsByIds(IEnumerable<Guid> ids, string userId, CancellationToken token = default);
    }
}
