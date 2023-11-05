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

        public async Task<IEnumerable<MessageData>> GetMessages(DateTimeOffset? maxTimestamp, int? pageSize, CancellationToken token = default)
        {
            return (await this.messageRepository.GetAll(maxTimestamp, pageSize, token)).Select(m =>
                new MessageData()
                {
                    Id = m.Id,
                    Content = m.Content,
                    Region = m.Region,
                    RegionId = m.RegionId,
                    Sender = m.Sender,
                    SenderId = m.SenderId,
                    CreationTimestamp = m.CreationTimestamp,
                    LastModificationTimestamp = m.LastModificationTimestamp
                }
            );
        }
    }
}
