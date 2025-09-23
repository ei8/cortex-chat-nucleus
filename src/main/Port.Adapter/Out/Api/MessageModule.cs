using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using Nancy;
using Nancy.Helpers;
using Nancy.Responses;
using neurUL.Common.Domain.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
{
    public class MessageModule : NancyModule
    {
        public MessageModule(IMessageQueryService messageQueryService) : base("/nuclei/chat/messages")
        {
            this.Get("/", async (parameters) => 
            {
                return await MessageModule.ProcessRequest(async () =>
                {
                    var messages = Enumerable.Empty<MessageResult>();
                    
                    if (Request.Query.avatarId.HasValue)
                        messages = await messageQueryService.GetMessages(
                                ((string)Request.Query.avatarId.Value.ToString())
                                .Split(',')
                                .Select(s => Guid.Parse(s)),
                            Request.Query.maxTimestamp.HasValue ?
                                DateTimeOffset.Parse(HttpUtility.UrlDecode(Request.Query.maxTimestamp.Value)) :
                                null,
                            Request.Query.pageSize.HasValue ?
                                int.Parse(Request.Query.pageSize.Value) :
                                null,
                            MessageModule.GetUserId(Request)
                        );
                    else
                        messages = await messageQueryService.GetMessages(
                            Request.Query.maxTimestamp.HasValue ?
                                DateTimeOffset.Parse(HttpUtility.UrlDecode(Request.Query.maxTimestamp.Value)) :
                                null,
                            Request.Query.pageSize.HasValue ?
                                int.Parse(Request.Query.pageSize.Value) :
                                null,
                            MessageModule.GetUserId(Request)
                        );

                    return new TextResponse(JsonConvert.SerializeObject(messages));
                });
            }
            );
        }

        // TODO: duplicated in ei8.Cortex.Library.Port.Adapter.Out.Api.NeuronModule etc.
        internal static string GetUserId(Request value)
        {
            AssertionConcern.AssertArgumentValid(k => k, (bool)value.Query["userid"].HasValue, "User Id was not found.", "userid");

            return value.Query["userid"].ToString();
        }

        // TODO: duplicated in ei8.Cortex.Library.Port.Adapter.Out.Api.NeuronModule etc.
        internal static async Task<Response> ProcessRequest(Func<Task<Response>> action)
        {
            var result = new Response { StatusCode = HttpStatusCode.OK };

            try
            {
                result = await action();
            }
            catch (Exception ex)
            {
                result = new TextResponse(HttpStatusCode.BadRequest, ex.ToString());
            }

            return result;
        }
    }
}