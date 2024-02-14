using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using System.Linq;
using System;
using ei8.Cortex.Chat.Nucleus.Application;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
{
    public class RegionModule : NancyModule
    {
        public RegionModule(IRegionQueryService regionQueryService, IIdentityService identityService) : base("/nuclei/chat/regions")
        {
            this.Get("/", async (parameters) => {
                identityService.UserId = MessageModule.GetUserId(Request);

                if (Request.Query.id.HasValue)
                    return new TextResponse(JsonConvert.SerializeObject(
                       await regionQueryService.GetRegionsByIds(
                            ((string)Request.Query.id.Value.ToString())
                            .Split(',')
                            .Select(s => Guid.Parse(s))
                       )
                       ));
                else
                    return new TextResponse(JsonConvert.SerializeObject(
                       await regionQueryService.GetRegions()
                       ));
            }
            );
        }
    }
}
