using CQRSlite.Commands;
using Nancy;
using ei8.Cortex.Chat.Nucleus.Application.Messages.Commands;
using System;
using neurUL.Common.Api;
using CQRSlite.Domain.Exception;

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

                            if (bodyAsDictionary.ContainsKey("RegionId"))
                                if (Guid.TryParse(bodyAsObject.RegionId.ToString(), out Guid tempRegionId))
                                    regionId = tempRegionId;

                            await commandSender.Send(new CreateMessage(
                                Guid.Parse(bodyAsObject.Id.ToString()),
                                bodyAsObject.Content.ToString(),
                                regionId,
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

            // TODO: this.Patch("/{neuronId}", async (parameters) =>
            //{
            //    return await this.Request.ProcessCommand(
            //            async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
            //            {
            //                ICommand result = null;
            //                if (bodyAsDictionary.ContainsKey("Tag"))
            //                    result = new ChangeNeuronTag(
            //                        Guid.Parse(parameters.neuronId),
            //                        bodyAsObject.Tag.ToString(),
            //                        bodyAsObject.UserId.ToString(),
            //                        expectedVersion
            //                        );
            //                else if (bodyAsDictionary.ContainsKey("RegionId"))
            //                    result = new ChangeNeuronRegionId(
            //                        Guid.Parse(parameters.neuronId),
            //                        bodyAsObject.RegionId == null ? null : bodyAsObject.RegionId.ToString(),
            //                        bodyAsObject.UserId.ToString(),
            //                        expectedVersion
            //                        );
            //                else if (bodyAsDictionary.ContainsKey("ExternalReferenceUrl"))
            //                    result = new ChangeNeuronExternalReferenceUrl(
            //                        Guid.Parse(parameters.neuronId),
            //                        bodyAsObject.ExternalReferenceUrl.ToString(),
            //                        bodyAsObject.UserId.ToString(),
            //                        expectedVersion
            //                        );
            //                await commandSender.Send(result);
            //            },
            //            ConcurrencyExceptionSetter,
            //            new string[] { "Tag", "RegionId", "ExternalReferenceUrl" },
            //            "UserId"
            //        );
            //}
            //);

            // TODO: this.Delete("/{neuronId}", async (parameters) =>
            //{
            //    return await this.Request.ProcessCommand(
            //            async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
            //            {
            //                await commandSender.Send(new DeactivateNeuron(
            //                    Guid.Parse(parameters.neuronId),
            //                    bodyAsObject.UserId.ToString(),
            //                    expectedVersion
            //                    ));
            //            },
            //            ConcurrencyExceptionSetter,
            //            new string[0],
            //            "UserId"
            //        );
            //}
            //);
        }
    }
}
