using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public static class ExtensionMethods
    {
        public static Common.MessageResult ToCommon(this Domain.Model.Messages.MessageResult value) =>
            new Common.MessageResult()
            {
                Id = value.Message.Id,
                Content = value.Message.Content,
                RegionTag = value.RegionTag,
                RegionId = value.Message.RegionId,
                SenderTag = value.SenderTag,
                SenderId = value.Message.SenderId,
                CreationTimestamp = value.Message.CreationTimestamp,
                UnifiedLastModificationTimestamp = value.Message.UnifiedLastModificationTimestamp,
                IsCurrentUserCreationAuthor = value.IsCurrentUserCreationAuthor
            };

        public static Domain.Model.Messages.MessageResult ToDomain(this Common.MessageResult value) =>
            new Domain.Model.Messages.MessageResult()
            {
                Message = new Message
                {
                    Id = value.Id,
                    Content = value.Content,
                    RegionId = value.RegionId,
                    SenderId = value.SenderId,
                    CreationTimestamp = value.CreationTimestamp,
                    UnifiedLastModificationTimestamp = value.UnifiedLastModificationTimestamp,
                },
                RegionTag = value.RegionTag,
                SenderTag = value.SenderTag,
                IsCurrentUserCreationAuthor = value.IsCurrentUserCreationAuthor
            };
    }
}
