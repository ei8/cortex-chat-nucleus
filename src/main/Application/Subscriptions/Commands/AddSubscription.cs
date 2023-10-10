// TODO: using CQRSlite.Commands;
//using ei8.Cortex.Subscriptions.Common;
//using ei8.Cortex.Subscriptions.Common.Receivers;

//namespace ei8.Cortex.Chat.Nucleus.Application.Subscriptions.Commands
//{
//    public class AddSubscription<T> : ICommand 
//        where T: IReceiverInfo
//    {
//        public AddSubscription(SubscriptionInfo subscriptionInfo, T receiverInfo, string userId, int expectedVersion)
//        {
//            this.SubscriptionInfo = subscriptionInfo;
//            this.ReceiverInfo = receiverInfo;
//            this.UserId = userId;
//            this.ExpectedVersion = expectedVersion;
//        }

//        public SubscriptionInfo SubscriptionInfo { get; private set; }
//        public T ReceiverInfo { get; private set; }
//        public string UserId { get; private set; }
//        public int ExpectedVersion { get; private set; }
//    }
//}
