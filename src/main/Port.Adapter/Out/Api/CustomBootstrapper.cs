using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Client.In;
using ei8.Cortex.Chat.Nucleus.Client.Out;
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
using System.Collections.Generic;
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
            container.Register<ISettingsService, SettingsService>();
            container.Register<IMessageQueryClient, HttpMessageQueryClient>();
            container.Register<IValidationClient, HttpValidationClient>();

            container.Resolve<ISettingsService>().Authorities = this.configuration.GetSection("Authorities").Get<IEnumerable<Authority>>();

            container.Register<IMessageReadRepository>(
                (tic, npo) =>
                    new HttpMessageReadRepository(
                        container.Resolve<INeuronQueryClient>(),
                        container.Resolve<IMessageQueryClient>(),
                        container.Resolve<ISettingsService>(),
                        this.serviceProvider.GetService<IHttpClientFactory>()
                        )
                    );
            container.Register<IMessageQueryService, MessageQueryService>();
        }
    }
}
