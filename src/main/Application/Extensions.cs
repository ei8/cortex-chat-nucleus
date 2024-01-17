using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Domain.Model;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public static class Extensions
    {
        public static Common.MessageData ToCommon(this Message value) =>
            new MessageData()
            {
                Id = value.Id,
                Content = value.Content,
                Region = value.Region,
                RegionId = value.RegionId,
                Sender = value.Sender,
                SenderId = value.SenderId,
                CreationTimestamp = value.CreationTimestamp,
                UnifiedLastModificationTimestamp = value.UnifiedLastModificationTimestamp,
                IsCurrentUserCreationAuthor = value.IsCurrentUserCreationAuthor
            };

        public static Message ToDomain(this MessageData value) =>
            new Message()
            {
                Id = value.Id,
                Content = value.Content,
                Region = value.Region,
                RegionId = value.RegionId,
                Sender = value.Sender,
                SenderId = value.SenderId,
                CreationTimestamp = value.CreationTimestamp,
                UnifiedLastModificationTimestamp = value.UnifiedLastModificationTimestamp,
                IsCurrentUserCreationAuthor = value.IsCurrentUserCreationAuthor
            };
    }
}
