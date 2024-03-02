using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application.Messages
{
    public class MessageQueryService : IMessageQueryService
    {
        private readonly IMessageReadRepository messageRepository;
        private readonly IAvatarReadRepository avatarRepository;

        public MessageQueryService(IMessageReadRepository messageRepository, IAvatarReadRepository avatarRepository)
        {
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(avatarRepository, nameof(avatarRepository));

            this.messageRepository = messageRepository;
            this.avatarRepository = avatarRepository;
        }

        public async Task<IEnumerable<Common.MessageResult>> GetMessages(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Guid> avatarIds, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(avatarIds, nameof(avatarIds));

            return (
                await this.messageRepository.GetAll(
                    maxTimestamp, 
                    pageSize,
                    await this.avatarRepository.GetByIds(avatarIds),
                    token
                )
            ).Select(m => m.ToCommon());
        }
    }
}
