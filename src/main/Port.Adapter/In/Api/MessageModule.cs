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
        public MessageModule(ICommandSender commandSender, IIdentityService identityService) : base("/nuclei/chat/messages")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        false,
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            Guid? regionId = null;
                            string externalReferenceUrl = null;
                            var destinationRegionIds = Array.Empty<string>();

                            if (bodyAsDictionary.ContainsKey(nameof(CreateMessage.RegionId)))
                                if (Guid.TryParse(bodyAsObject.RegionId.ToString(), out Guid tempRegionId))
                                    regionId = tempRegionId;

                            if (bodyAsDictionary.ContainsKey(nameof(CreateMessage.ExternalReferenceUrl)))
                                externalReferenceUrl = bodyAsObject.ExternalReferenceUrl.ToString();

                            if (bodyAsDictionary.ContainsKey(nameof(CreateMessage.DestinationRegionIds)) &&
                                bodyAsDictionary[nameof(CreateMessage.DestinationRegionIds)] != null)
                                destinationRegionIds = ((JArray) bodyAsDictionary[nameof(CreateMessage.DestinationRegionIds)]).ToObject<string[]>();

                            identityService.UserId = bodyAsObject.UserId.ToString();

                            await commandSender.Send(new CreateMessage(
                                Guid.Parse(bodyAsObject.Id.ToString()),
                                bodyAsObject.Content.ToString(),
                                regionId,
                                externalReferenceUrl,
                                destinationRegionIds.Select(dri => Guid.Parse(dri))
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
