// TODO: using ei8.Cortex.Subscriptions.Client.Out;
//using ei8.Cortex.Subscriptions.Common;
//using System.Threading.Tasks;

//namespace ei8.Cortex.Chat.Nucleus.Application.Subscriptions
//{
//    public class SubscriptionConfigurationQueryService : ISubscriptionConfigurationQueryService
//    {
//        private readonly ISubscriptionsConfigurationClient client;
//        private readonly ISettingsService settings;

//        public SubscriptionConfigurationQueryService(ISubscriptionsConfigurationClient client, ISettingsService settings)
//        {
//            this.client = client;
//            this.settings = settings;
//        }

//        public async Task<SubscriptionConfiguration> GetServerConfigurationAsync() => 
//            await this.client.GetServerConfigurationAsync(settings.SubscriptionsOutBaseUrl);
//    }
//}
