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
        private readonly IRegionReadRepository regionRepository;

        public MessageQueryService(IMessageReadRepository messageRepository, IRegionReadRepository regionRepository)
        {
            AssertionConcern.AssertArgumentNotNull(messageRepository, nameof(messageRepository));
            AssertionConcern.AssertArgumentNotNull(regionRepository, nameof(regionRepository));

            this.messageRepository = messageRepository;
            this.regionRepository = regionRepository;
        }

        public async Task<IEnumerable<Common.MessageResult>> GetMessages(DateTimeOffset? maxTimestamp, int? pageSize, IEnumerable<Guid> externalRegionIds, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(externalRegionIds, nameof(externalRegionIds));

            return (
                await this.messageRepository.GetAll(
                    maxTimestamp, 
                    pageSize,
                    await this.regionRepository.GetByIds(externalRegionIds),
                    token
                )
            ).Select(m => m.ToCommon());
        }
    }
}
