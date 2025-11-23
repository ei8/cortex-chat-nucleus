using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Application.Avatars;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization.Implementation;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Model.Reflection;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.Coding.Persistence.Wrappers;
using ei8.Cortex.IdentityAccess.Client.In;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Library.Client.Out;
using ei8.Extensions.DependencyInjection;
using ei8.Extensions.DependencyInjection.Coding;
using ei8.Extensions.DependencyInjection.Coding.d23.neurULization;
using ei8.Extensions.DependencyInjection.Coding.d23.neurULization.Persistence;
using ei8.Extensions.DependencyInjection.Coding.Persistence;
using ei8.Extensions.DependencyInjection.Cortex;
using ei8.Extensions.DependencyInjection.EventSourcing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nancy;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.In.Api
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

            Trace.Listeners.Add(new ConsoleTraceListener());

            container.AddReadWriteCache();
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
            container.Register<IPermitClient, HttpPermitClient>();
            container.Register<INetworkTransactionData, NetworkTransactionData>();
            container.Register<INetworkTransactionService, NetworkTransactionService>();
            container.Register<INeuronQueryClient, HttpNeuronQueryClient>();
            var ss = container.Resolve<ISettingsService>();
            container.AddNetworkRepository(ss.CortexLibraryOutBaseUrl + "/", ss.QueryResultLimit, ss.AppUserId);
            Guid? appUserNeuronId = container.GetAppUserNeuronId();

            container.Register<IGrannyService, GrannyService>();
            container.AddTransactions(ss.EventSourcingInBaseUrl + "/", ss.EventSourcingOutBaseUrl + "/");
            container.AddDataAdapters(typeof(MessageCommandHandlers));
            container.Register<IMirrorRepository, MirrorRepository>();

            container.RegisterStagedAppMirrors(
                ss.InitializeMissingMirrors, 
                appUserNeuronId, 
                ss.InitializeRetryCount, 
                ss.InitializeRetryDelay,
                "Dependencies registration",
                MirrorRepositoryExtensions.ConvertToStringKeys(
                    ReflectionExtensions.GetMirrorKeys(
                        Common.Constants.InitMirrorKeyTypes
                    )
                ),
                (cpc) => cpc.PreRegisterAppMirrorSet(
                    () => CustomBootstrapper.RegisterCore(cpc, appUserNeuronId)
                )
            );
        }

        private static void RegisterCore(TinyIoCContainer container, Guid? appUserNeuronId)
        {
            container.Register<INetworkDictionary<string>>(new NetworkDictionary<string>());
            container.AddWriters();
            container.AddReaders();

            var grannyResults = Task.Run(async () => await container.AddInstantiatiesClassGranniesAsync(
                appUserNeuronId,
                Common.Constants.InitMirrorKeyTypes
            )).Result;

            container.Register<IClassInstanceNeuronsRetriever, ClassInstanceNeuronsRetriever>();
            container.Register<IIdInstanceNeuronsRetriever, IdInstanceNeuronsRetriever>();
            container.Register<IAvatarReadRepository, HttpAvatarReadRepository>();
            container.Register<Id23neurULizerOptions, neurULizerOptions>();
            container.Register<IneurULizer, neurULizer>();
            container.Register<IStringWrapperWriteRepository, StringWrapperWriteRepository>();
            container.Register<IMessageWriteRepository, HttpMessageWriteRepository>();
            container.Register<ICommunicatorWriteRepository<Sender>, HttpCommunicatorWriteRepository<Sender>>();
            container.Register<ICommunicatorWriteRepository<Recipient>, HttpCommunicatorWriteRepository<Recipient>>();
            container.Register<IWriteCacheService, WriteCacheService>();
            container.Register<MessageCommandHandlers>();
            container.Register<IAvatarWriteRepository, HttpAvatarWriteRepository>();
            container.Register<AvatarCommandHandlers>();
        }
    }
}
