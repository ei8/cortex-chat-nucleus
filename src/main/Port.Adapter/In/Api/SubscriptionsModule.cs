// TODO: using CQRSlite.Commands;
//using ei8.Cortex.Chat.Nucleus.Application.Subscriptions.Commands;
//using ei8.Cortex.Subscriptions.Common;
//using ei8.Cortex.Subscriptions.Common.Receivers;
//using Nancy;
//using neurUL.Common.Api;
//using System;
//using System.Threading.Tasks;

//namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.In.Api
//{
//    public class SubscriptionsModule : NancyModule
//    {
//        public SubscriptionsModule(ICommandSender commandSender) : base("/nuclei/d23/subscriptions")
//        {
//            this.Post("/receivers/{receiverType}", async (parameters) =>
//            {
//                return await this.Request.ProcessCommand(
//                        false,
//                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
//                        {
//                            await SendGenericRequestAsync(commandSender, bodyAsObject, parameters.receiverType, expectedVersion);
//                        },
//                        MessageModule.ConcurrencyExceptionSetter,
//                        new string[0],
//                        "SubscriptionInfo",
//                        "ReceiverInfo",
//                        "UserId"
//                        );
//            });
//        }

//        private async Task SendGenericRequestAsync(ICommandSender commandSender, dynamic bodyAsObject, string receiverType, int expectedVersion)
//        {
//            var subscriptionInfo = new SubscriptionInfo()
//            {
//                AvatarUrl = bodyAsObject.SubscriptionInfo.AvatarUrl,
//            };

//            switch (receiverType)
//            {
//                case "web":
//                    var receiverInfo = new BrowserReceiverInfo()
//                    {
//                        Name = bodyAsObject.ReceiverInfo.Name,
//                        PushAuth = bodyAsObject.ReceiverInfo.PushAuth,
//                        PushEndpoint = bodyAsObject.ReceiverInfo.PushEndpoint,
//                        PushP256DH = bodyAsObject.ReceiverInfo.PushP256DH,
//                    };
//                    await commandSender.Send(new AddSubscription<BrowserReceiverInfo>(subscriptionInfo, receiverInfo, bodyAsObject.UserId.ToString(), expectedVersion));
//                    break;
//                case "smtp":
//                    var receiverInfo2 = new SmtpReceiverInfo()
//                    {
//                        EmailAddress = bodyAsObject.ReceiverInfo.EmailAddress
//                    };
//                    await commandSender.Send(new AddSubscription<SmtpReceiverInfo>(subscriptionInfo, receiverInfo2, bodyAsObject.UserId.ToString(), expectedVersion));
//                    break;

//                default:
//                    throw new NotSupportedException($"Unsupported receiver type for endpoint {receiverType}");
//            }
//        }
//    }
//}
