using CQRSlite.Commands;
using CQRSlite.Domain.Exception;
using ei8.Cortex.Chat.Nucleus.Application.Avatars.Commands;
using Nancy;
using neurUL.Common.Api;
using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.In.Api
{
    /// <summary>
    /// Represents an AvatarModule that defines related routes and actions.
    /// </summary>
    public class AvatarModule : NancyModule
    {
        internal static readonly Func<Exception, HttpStatusCode> ConcurrencyExceptionSetter = new Func<Exception, HttpStatusCode>((ex) => { 
            HttpStatusCode result = HttpStatusCode.BadRequest;
            if (ex is ConcurrencyException)
                result = HttpStatusCode.Conflict;
            return result;
        });

        /// <summary>
        /// Constructs an AvatarModule.
        /// </summary>
        /// <param name="commandSender"></param>
        public AvatarModule(ICommandSender commandSender) : base("/nuclei/chat/avatars")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        false,
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            Guid? regionId = null;
                            string mirrorUrl = null;
                           
                            if (bodyAsDictionary.ContainsKey(nameof(CreateAvatar.RegionId)))
                                if (Guid.TryParse(bodyAsObject.RegionId.ToString(), out Guid tempRegionId))
                                    regionId = tempRegionId;

                            if (bodyAsDictionary.ContainsKey(nameof(CreateAvatar.MirrorUrl)))
                                mirrorUrl = bodyAsObject.MirrorUrl.ToString();

                            await commandSender.Send(new CreateAvatar(
                                Guid.Parse(bodyAsObject.Id.ToString()),
                                bodyAsObject.Name.ToString(),
                                regionId,
                                mirrorUrl,
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
                        "Name",
                        "UserId"
                    );
            }
            );
        }
    }
}
