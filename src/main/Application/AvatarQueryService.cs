using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Coding;
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
        private readonly Network readNetworkCache;

        public AvatarQueryService(
            IAvatarReadRepository avatarRepository,
            Network readNetworkCache
        )
        {
            AssertionConcern.AssertArgumentNotNull(avatarRepository, nameof(avatarRepository));
            AssertionConcern.AssertArgumentNotNull(readNetworkCache, nameof(readNetworkCache));

            this.avatarRepository = avatarRepository;
            this.readNetworkCache = readNetworkCache;
        }

        public async Task<IEnumerable<Common.AvatarResult>> GetAvatars(string userId, CancellationToken token = default)
        {
            // TODO:1 validate if user has access to neurons in result using validationClient
            return (await this.avatarRepository.GetAll(token))
                .Select(a => 
                    this.readNetworkCache.GetValidateNeuron(
                        a.Id,
                        n => new Common.AvatarResult()
                        {
                            Id = a.Id,
                            Name = a.Name,
                            MirrorUrl = n.MirrorUrl,
                            Url = n.Url
                        }
                    )
                );
        }

        public async Task<IEnumerable<Common.AvatarResult>> GetAvatarsByIds(IEnumerable<Guid> ids, string userId, CancellationToken token = default)
        {
            // TODO:1 validate if user has access to neurons in result using validationClient
            return (await this.avatarRepository.GetByIds(ids, token))
                .Select(a =>
                    this.readNetworkCache.GetValidateNeuron(
                        a.Id,
                        n => new Common.AvatarResult()
                        {
                            Id = a.Id,
                            Name = a.Name,
                            MirrorUrl = n.MirrorUrl,
                            Url = n.Url
                        }
                    )
                );
        }
    }
}
