using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.Graph.Client;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.Out;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Subscriptions.Client.Out;
using System.Net.Http;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
// TODO: using ei8.Cortex.Chat.Nucleus.Application.Subscriptions;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        public CustomBootstrapper()
        {
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.Register<IRequestProvider>(
               (tic, npo) =>
               {
                   var rp = new RequestProvider();
                   rp.SetHttpClientHandler(new HttpClientHandler());
                   return rp;
               });

            container.Register<INeuronGraphQueryClient, HttpNeuronGraphQueryClient>();
            container.Register<ISettingsService, SettingsService>();
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<IEventSerializer, EventSerializer>();
            container.Register<IMessageRepository, HttpMessageRepository>();
            container.Register<IMessageQueryService, MessageQueryService>();
            // TODO: container.Register<ISubscriptionConfigurationQueryService, SubscriptionConfigurationQueryService>();
            container.Register<ISubscriptionsConfigurationClient, HttpSubscriptionConfigurationClient>();
        }
    }
}
