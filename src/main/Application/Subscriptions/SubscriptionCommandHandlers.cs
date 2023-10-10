// TODO: using CQRSlite.Commands;
//using ei8.Cortex.Chat.Nucleus.Application.Subscriptions.Commands;
//using ei8.Cortex.IdentityAccess.Client.Out;
//using ei8.Cortex.Subscriptions.Client.In;
//using ei8.Cortex.Subscriptions.Common;
//using ei8.Cortex.Subscriptions.Common.Receivers;
//using neurUL.Common.Domain.Model;
//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ei8.Cortex.Chat.Nucleus.Application.Subscriptions
//{
//    public class SubscriptionCommandHandlers :
//        ICancellableCommandHandler<AddSubscription<BrowserReceiverInfo>>,
//        ICancellableCommandHandler<AddSubscription<SmtpReceiverInfo>>
//    {
//        private readonly ISubscriptionsClient subscriptionsClient;
//        private readonly IValidationClient validationClient;
//        private readonly ISettingsService settingsService;

//        public SubscriptionCommandHandlers(ISubscriptionsClient subscriptionsClient, IValidationClient validationClient,
//            ISettingsService settingsService)
//        {
//            this.subscriptionsClient = subscriptionsClient;
//            this.validationClient = validationClient;
//            this.settingsService = settingsService;
//        }

//        public async Task Handle(AddSubscription<BrowserReceiverInfo> message, CancellationToken token = default)
//        {
//            AssertionConcern.AssertArgumentNotNull(message, nameof(message));
//            AssertionConcern.AssertArgumentNotNull(message.ReceiverInfo, nameof(message.ReceiverInfo));
//            AssertionConcern.AssertArgumentNotNull(message.SubscriptionInfo, nameof(message.SubscriptionInfo));

//            var validationResult = await this.validationClient.ReadNeurons(
//                this.settingsService.IdentityAccessOutBaseUrl + "/",
//                Enumerable.Empty<Guid>(),
//                message.UserId,
//                token
//                );

//            if (!validationResult.HasErrors)
//            {
//                message.SubscriptionInfo.UserNeuronId = validationResult.UserNeuronId;

//                var request = this.BuildRequestObject(message.SubscriptionInfo, message.ReceiverInfo);

//                await this.subscriptionsClient.AddSubscription(settingsService.SubscriptionsInBaseUrl, request, token);
//            }
//        }

//        public async Task Handle(AddSubscription<SmtpReceiverInfo> message, CancellationToken token = default)
//        {
//            AssertionConcern.AssertArgumentNotNull(message, nameof(message));
//            AssertionConcern.AssertArgumentNotNull(message.ReceiverInfo, nameof(message.ReceiverInfo));
//            AssertionConcern.AssertArgumentNotNull(message.SubscriptionInfo, nameof(message.SubscriptionInfo));

//            var validationResult = await this.validationClient.ReadNeurons(
//                this.settingsService.IdentityAccessOutBaseUrl + "/",
//                Enumerable.Empty<Guid>(),
//                message.UserId,
//                token
//                );

//            if (!validationResult.HasErrors)
//            {
//                message.SubscriptionInfo.UserNeuronId = validationResult.UserNeuronId;

//                var request = this.BuildRequestObject(message.SubscriptionInfo, message.ReceiverInfo);

//                await this.subscriptionsClient.AddSubscription(settingsService.SubscriptionsInBaseUrl, request, token);
//            }
//        }

//        private IAddSubscriptionReceiverRequest<T> BuildRequestObject<T>(SubscriptionInfo subscriptionInfo, T receiverInfo) where T : IReceiverInfo
//        {
//            switch (receiverInfo)
//            {
//                case BrowserReceiverInfo br:
//                    return (IAddSubscriptionReceiverRequest<T>)new AddSubscriptionWebReceiverRequest()
//                    {
//                        SubscriptionInfo = subscriptionInfo,
//                        ReceiverInfo = br
//                    };
//                case SmtpReceiverInfo sr:
//                    return (IAddSubscriptionReceiverRequest<T>)new AddSubscriptionSmtpReceiverRequest()
//                    {
//                        SubscriptionInfo = subscriptionInfo,
//                        ReceiverInfo = sr
//                    };

//                default:
//                    throw new NotSupportedException($"Unsupported receiver request type: {receiverInfo.GetType().Name}");
//            }
//        }
//    }
//}
