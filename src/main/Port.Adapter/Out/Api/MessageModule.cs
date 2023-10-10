﻿using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using neurUL.Common;
using System.Linq;
using System.Text;
using ei8.Cortex.Chat.Common;
using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
{
    public class MessageModule : NancyModule
    {
        public MessageModule(IMessageQueryService messageQueryService) : base("/nuclei/chat/messages")
        {
            this.Get("/", async (parameters) =>
                 new TextResponse(JsonConvert.SerializeObject(
                    await messageQueryService.GetMessages(
                        Request.Query.maxTimestamp.HasValue ?
                            DateTimeOffset.Parse(Request.Query.maxTimestamp.Value) :
                            null,
                        Request.Query.pageSize.HasValue ?
                            int.Parse(Request.Query.pageSize.Value) :
                            null
                    )
                    ))
            );
        }
    }
}