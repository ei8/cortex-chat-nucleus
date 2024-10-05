using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public static class ExtensionMethods
    {
        public static Common.MessageResult ToCommon(this MessageResult value) =>
            new Common.MessageResult()
            {
                Id = value.Message.Id,
                Content = value.Message.Content,
                RegionTag = value.RegionTag,
                RegionId = value.Message.RegionId,
                SenderTag = value.SenderTag,
                SenderId = value.Message.SenderId,
                ExternalReferenceUrl = value.Message.ExternalReferenceUrl,
                CreationTimestamp = value.Message.CreationTimestamp,
                UnifiedLastModificationTimestamp = value.Message.UnifiedLastModificationTimestamp,
                IsCurrentUserCreationAuthor = value.IsCurrentUserSender
            };

        public static MessageResult ToDomain(this Common.MessageResult value) =>
            new MessageResult()
            {
                Message = new Message
                {
                    Id = value.Id,
                    Content = value.Content,
                    RegionId = value.RegionId,
                    SenderId = value.SenderId,
                    ExternalReferenceUrl = value.ExternalReferenceUrl,
                    CreationTimestamp = value.CreationTimestamp,
                    UnifiedLastModificationTimestamp = value.UnifiedLastModificationTimestamp
                },
                RegionTag = value.RegionTag,
                SenderTag = value.SenderTag,
                IsCurrentUserSender = value.IsCurrentUserCreationAuthor
            };

        public static Common.AvatarResult ToCommon(this Avatar value) =>
            new Common.AvatarResult()
            {
                Id = value.Id,
                Name = value.Name,
                ExternalReferenceUrl = value.ExternalReferenceUrl,
                Url = value.Url
            };
    }
}
