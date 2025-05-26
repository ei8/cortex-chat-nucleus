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

        public AvatarQueryService(
            IAvatarReadRepository avatarRepository
        )
        {
            AssertionConcern.AssertArgumentNotNull(avatarRepository, nameof(avatarRepository));

            this.avatarRepository = avatarRepository;
        }

        public async Task<IEnumerable<Common.AvatarResult>> GetAvatars(string userId, CancellationToken token = default)
        {
            // TODO:1 validate if user has access to neurons in result using validationClient
            return (await this.avatarRepository.GetAll(token))
                .Select(a =>
                    new Common.AvatarResult()
                    {
                        Id = a.Id,
                        Name = a.Name,
                        MirrorUrl = a.MirrorUrl,
                        Url = a.Url
                    }
                );
        }

        public async Task<IEnumerable<Common.AvatarResult>> GetAvatarsByIds(IEnumerable<Guid> ids, string userId, CancellationToken token = default)
        {
            // TODO:1 validate if user has access to neurons in result using validationClient
            return (await this.avatarRepository.GetByIds(ids, token))
                .Select(a =>
                    new Common.AvatarResult()
                    {
                        Id = a.Id,
                        Name = a.Name,
                        MirrorUrl = a.MirrorUrl,
                        Url = a.Url
                    }
                );
        }
    }
}
