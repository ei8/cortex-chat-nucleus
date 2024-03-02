using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public class AvatarQueryService : IAvatarQueryService
    {
        private readonly IAvatarReadRepository avatarRepository;

        public AvatarQueryService(IAvatarReadRepository avatarRepository)
        {
            AssertionConcern.AssertArgumentNotNull(avatarRepository, nameof(avatarRepository));

            this.avatarRepository = avatarRepository;
        }

        public async Task<IEnumerable<AvatarResult>> GetAvatars(CancellationToken token = default) => 
            (await this.avatarRepository.GetAll(token)).Select(r => r.ToCommon());

        public async Task<IEnumerable<AvatarResult>> GetAvatarsByIds(IEnumerable<Guid> ids, CancellationToken token = default)
        {
            return (await this.avatarRepository.GetByIds(ids, token)).Select(r => r.ToCommon());
        }
    }
}
