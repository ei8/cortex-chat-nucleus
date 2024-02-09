using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Library.Client.Out;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using System;
using System.Net.Http;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;

        public CustomBootstrapper(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;
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
            container.Register(this.configuration);
            container.Register<ISettingsService, SettingsService>();
            container.Register<IIdentityService, IdentityService>();
            container.Register<IRegionReadRepository, HttpRegionReadRepository>();
            container.Register<IMessageQueryClient, HttpMessageQueryClient>();
            container.Register<IValidationClient, HttpValidationClient>();

            container.Register<IMessageReadRepository>(
                (tic, npo) =>
                    new HttpMessageReadRepository(
                        container.Resolve<INeuronQueryClient>(),
                        container.Resolve<IMessageQueryClient>(),
                        container.Resolve<ISettingsService>(),
                        this.serviceProvider.GetService<IHttpClientFactory>(),
                        container.Resolve<IIdentityService>()
                        )
                    );
            container.Register<IMessageQueryService, MessageQueryService>();
        }
    }
}
