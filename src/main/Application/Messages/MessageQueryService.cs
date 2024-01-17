using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
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

        public MessageQueryService(IMessageReadRepository messageRepository)
        {
            this.messageRepository = messageRepository;
        }

        public async Task<IEnumerable<MessageData>> GetMessages(DateTimeOffset? maxTimestamp, int? pageSize, string userId, CancellationToken token = default)
        {
            return (
                await this.messageRepository.GetAll(
                    maxTimestamp, 
                    pageSize, 
                    userId, 
                    token
                )
            ).Select(m => m.ToCommon());
        }
    }
}
