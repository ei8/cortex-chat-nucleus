using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Library.Client.Out;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using System.Net.Http;

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

            container.Register<INeuronQueryClient, HttpNeuronQueryClient>();
            container.Register<ISettingsService, SettingsService>();
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<IMessageReadRepository, HttpMessageReadRepository>();
            container.Register<IMessageQueryService, MessageQueryService>();
        }
    }
}
