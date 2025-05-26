using CQRSlite.Commands;
using CQRSlite.Domain.Exception;
using ei8.Cortex.Chat.Nucleus.Application.Messages.Commands;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using Nancy;
using neurUL.Common.Api;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.In.Api
{
    public class MessageModule : NancyModule
    {
        internal static readonly Func<Exception, HttpStatusCode> ConcurrencyExceptionSetter = new Func<Exception, HttpStatusCode>((ex) => { 
                            HttpStatusCode result = HttpStatusCode.BadRequest;             
                            if (ex is ConcurrencyException)
                                result = HttpStatusCode.Conflict;                            
                            return result;
                        });
        public MessageModule(ICommandSender commandSender) : base("/nuclei/chat/messages")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        false,
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            Guid? regionId = null;
                            string mirrorUrl = null;
                            string[] recipientAvatarIds = null;

                            if (bodyAsDictionary.ContainsKey(nameof(CreateMessage.RegionId)))
                                if (Guid.TryParse(bodyAsObject.RegionId.ToString(), out Guid tempRegionId))
                                    regionId = tempRegionId;

                            if (bodyAsDictionary.ContainsKey(nameof(CreateMessage.MirrorUrl)))
                                mirrorUrl = bodyAsObject.MirrorUrl.ToString();

                            if (bodyAsDictionary.ContainsKey(nameof(CreateMessage.RecipientAvatarIds)) &&
                                bodyAsDictionary[nameof(CreateMessage.RecipientAvatarIds)] != null)
                                recipientAvatarIds = ((JArray) bodyAsDictionary[nameof(CreateMessage.RecipientAvatarIds)]).ToObject<string[]>();

                            await commandSender.Send(new CreateMessage(
                                Guid.Parse(bodyAsObject.Id.ToString()),
                                bodyAsObject.Content.ToString(),
                                regionId,
                                mirrorUrl,
                                recipientAvatarIds?.Select(dri => Guid.Parse(dri)),
                                bodyAsObject.UserId.ToString()
                                )
                            );
                        },
                        (ex) => {
                            // TODO: immediately cause calling Polly to fail (handle specific failure http code to signal "it's not worth retrying"?)
                            // i.e. there is an issue with the data
                            HttpStatusCode result = HttpStatusCode.BadRequest;
                            if (ex is ConcurrencyException)
                                result = HttpStatusCode.Conflict;
                            return result;
                        },
                        Array.Empty<string>(),
                        "Id",
                        "Content",
                        "UserId"
                    );
            }
            );
        }
    }
}
