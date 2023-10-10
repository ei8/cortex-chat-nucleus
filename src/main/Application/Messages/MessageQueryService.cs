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
        private readonly IMessageRepository messageRetrievalService;

        public MessageQueryService(IMessageRepository messageRetrievalService)
        {
            this.messageRetrievalService = messageRetrievalService;
        }

        public async Task<IEnumerable<MessageData>> GetMessages(DateTimeOffset? maxTimestamp, int? pageSize, CancellationToken token = default)
        {
            return (await this.messageRetrievalService.GetAll(maxTimestamp, pageSize, token)).Select(m =>
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
