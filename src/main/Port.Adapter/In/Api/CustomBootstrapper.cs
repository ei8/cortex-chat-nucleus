using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.Common;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.IdentityAccess.Client.In;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Library.Client.Out;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.In.Api
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

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<IDictionary<string, Ensemble>>(new Dictionary<string, Ensemble>());
            container.Register(this.serviceProvider.GetService<IOptions<List<ExternalReference>>>());
            // TODO: remove when Authorities is removed from settings
            container.Register(this.configuration);
            container.Register<ISettingsService, SettingsService>();

            container.Register(
                container.CreateTransientEnsembleRepository().GetPrimitives(
                    container.Resolve<ISettingsService>().AppUserId,
                    container.Resolve<ISettingsService>().CortexLibraryOutBaseUrl
                ).Result
            );
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.Register(this.serviceProvider.GetService<IHttpClientFactory>());
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<IAvatarReadRepository, HttpAvatarReadRepository>();
            container.Register<IPermitClient, HttpPermitClient>();
            container.Register<IRecipientWriteRepository, HttpRecipientWriteRepository>();
            container.Register<IEnsembleTransactionService, EnsembleTransactionService>();
            container.AddRequestProvider();
            container.Register<INeuronQueryClient, HttpNeuronQueryClient>();
            container.Register<IEnsembleRepository, EnsembleRepository>();
            container.Register<IGrannyService, GrannyService>();
            container.Addd23neurULizerOptions();
            container.Register<IneurULizer, neurULizer>();
            container.Register<IMessageWriteRepository, HttpMessageWriteRepository>();
            container.Register<MessageCommandHandlers>();

            container.AddTransactions();
            container.AddWriters();
            container.AddReaders();
            container.AddDataAdapters(typeof(MessageCommandHandlers));
        }
    }
}
