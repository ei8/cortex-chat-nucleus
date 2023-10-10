// TODO: using ei8.Cortex.Chat.Nucleus.Application.Subscriptions;
//using Nancy;
//using Nancy.Responses;
//using Newtonsoft.Json;

//namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
//{
//    public class SubscriptionModule : NancyModule
//    {
//        public SubscriptionModule(ISubscriptionConfigurationQueryService service) : base("/nuclei/d23/subscriptions")
//        {
//            this.Get("/config", async (parameters) =>
//            {
//                var result = await service.GetServerConfigurationAsync();

//                return new TextResponse(JsonConvert.SerializeObject(result));
//            });
//        }
//    }
//}
