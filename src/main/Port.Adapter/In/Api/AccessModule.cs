// TODO: using ei8.Cortex.Chat.Nucleus.Application.Access;
//using Nancy;
//using neurUL.Common.Api;
//using System;

//namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.In.Api
//{
//    public class AccessModule : NancyModule
//    {
//        internal static readonly Func<Exception, HttpStatusCode> ExceptionSetter = new Func<Exception, HttpStatusCode>(ex => HttpStatusCode.InternalServerError);

//        public AccessModule(IAccessApplicationService accessApplicationService) : base("/nuclei/d23/access")
//        {
//            this.Post("/neuron/{neuronId}", async (parameters) =>
//            {
//                return await this.Request.ProcessCommand(
//                          false,
//                          async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
//                          {
//                              await accessApplicationService.CreateNeuronAccessRequest(Guid.Parse(parameters.neuronId), Guid.Parse(bodyAsObject.UserId.ToString()));
//                          },
//                          ExceptionSetter,
//                          new string[0],
//                          new string[] { "UserId" }
//                      );
//            });
//        }
//    }
//}
