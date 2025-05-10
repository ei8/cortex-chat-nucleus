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

        public async Task<IEnumerable<Common.MessageResult>> GetMessages(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Guid> avatarIds, string userId, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(avatarIds, nameof(avatarIds));

            var avatars = await this.avatarRepository.GetByIds(avatarIds);

            // TODO:1 Using validationClient, validate neurons against userId used during deneurULization prior to returning results
            return (
                await this.messageRepository.GetAll(
                    maxTimestamp, 
                    pageSize,
                    avatars,
                    token
                )
            ).Select(m => m.ToCommon());
        }
    }
}
