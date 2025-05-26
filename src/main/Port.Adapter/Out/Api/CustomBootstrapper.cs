using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.Common;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.Coding.Persistence.Wrappers;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Library.Client.Out;
using ei8.Extensions.DependencyInjection;
using ei8.Extensions.DependencyInjection.Coding.d23.neurULization;
using ei8.Extensions.DependencyInjection.Coding.d23.neurULization.Persistence;
using ei8.Extensions.DependencyInjection.Coding.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nancy;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        private readonly IServiceProvider serviceProvider;
        
        public CustomBootstrapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<IDictionary<string, Network>>(new Dictionary<string, Network>());
            container.Register<IDictionary<string, IGranny>>(new Dictionary<string, IGranny>());
            container.Register(this.serviceProvider.GetService<IOptions<List<MirrorConfig>>>());
            container.Register(this.serviceProvider.GetService<IOptions<List<Authority>>>());
            container.Register<ISettingsService, SettingsService>();
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.AddRequestProvider();
            container.Register(this.serviceProvider.GetService<IHttpClientFactory>());
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<IClassInstanceNeuronsRetriever, ClassInstanceNeuronsRetriever>();
            container.Register<IIdInstanceNeuronsRetriever, IdInstanceNeuronsRetriever>();
            container.Register<IAvatarReadRepository, HttpAvatarReadRepository>();
            container.Register<IMessageQueryClient, HttpMessageQueryClient>();
            container.Register<INetworkTransactionData, NetworkTransactionData>();
            container.Register<INetworkTransactionService, NetworkTransactionService>();
            container.Register<INeuronQueryClient, HttpNeuronQueryClient>();
            var ss = container.Resolve<ISettingsService>();
            container.AddNetworkRepository(ss.CortexLibraryOutBaseUrl + "/", ss.QueryResultLimit, ss.AppUserId);
            container.AddGrannyService(ss.IdentityAccessOutBaseUrl + "/", ss.AppUserId);
            container.AddTransactions(ss.EventSourcingInBaseUrl + "/", ss.EventSourcingOutBaseUrl + "/");
            container.AddDataAdapters();
            container.Register<IMirrorRepository, MirrorRepository>();

            var result = Task.Run(() => container.AddMirrors(
                Default.InitMirrorKeys,
                false,
                Guid.Empty
            )).Result;

            if (result)
            {
                container.Register<Id23neurULizerOptions, neurULizerOptions>();
                container.Register<IneurULizer, neurULizer>();
                container.Register<IStringWrapperRepository, StringWrapperRepository>();
                container.Register<IMessageReadRepository, HttpMessageReadRepository>();
                container.Register<IMessageQueryService, MessageQueryService>();
                container.Register<IAvatarQueryService, AvatarQueryService>();
                container.AddWriters();
                container.AddReaders();
            }
        }
    }
}
