using ei8.Cortex.Chat.Nucleus.Application.Avatars;
using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
{
    public class AvatarModule : NancyModule
    {
        public AvatarModule(IAvatarQueryService avatarQueryService) : base("/nuclei/chat/avatars")
        {
            this.Get("/", async (parameters) => {
                var userId = MessageModule.GetUserId(Request);

                if (Request.Query.id.HasValue)
                    return new TextResponse(JsonConvert.SerializeObject(
                       await avatarQueryService.GetAvatarsByIds(
                            ((string)Request.Query.id.Value.ToString())
                                .Split(',')
                                .Select(s => Guid.Parse(s)),
                            userId
                       )
                       ));
                else
                    return new TextResponse(JsonConvert.SerializeObject(
                       await avatarQueryService.GetAvatars(userId)
                       ));
            }
            );
        }
    }
}
