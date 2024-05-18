using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using Nancy;
using Nancy.Helpers;
using Nancy.Responses;
using neurUL.Common.Domain.Model;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
{
    public class MessageModule : NancyModule
    {
        public MessageModule(IMessageQueryService messageQueryService) : base("/nuclei/chat/messages")
        {
            this.Get("/", async (parameters) => {
                return new TextResponse(JsonConvert.SerializeObject(
                   await messageQueryService.GetMessages(
                       Request.Query.maxTimestamp.HasValue ?
                           DateTimeOffset.Parse(HttpUtility.UrlDecode(Request.Query.maxTimestamp.Value)) :
                           null,
                       Request.Query.pageSize.HasValue ?
                           int.Parse(Request.Query.pageSize.Value) :
                           null,
                       Request.Query.avatarId.HasValue ?
                           ((string) Request.Query.avatarId.Value.ToString())
                            .Split(',')
                            .Select(s => Guid.Parse(s)):
                           Array.Empty<Guid>(),
                       MessageModule.GetUserId(Request)
                   )
                   ));
                }
            );
        }

        // TODO: duplicated in ei8.Cortex.Library.Port.Adapter.Out.Api.NeuronModule etc.
        internal static string GetUserId(Request value)
        {
            AssertionConcern.AssertArgumentValid(k => k, (bool)value.Query["userid"].HasValue, "User Id was not found.", "userid");

            return value.Query["userid"].ToString();
        }
    }
}