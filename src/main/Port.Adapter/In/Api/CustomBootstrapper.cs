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
using ei8.Cortex.Coding.d23.neurULization.Persistence.Versioning;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.Coding.Persistence.Versioning;
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
            container.AddGrannyService(ss.IdentityAccessOutBaseUrl + "/", ss.AppUserId);
            container.AddTransactions(ss.EventSourcingInBaseUrl + "/", ss.EventSourcingOutBaseUrl + "/");
            container.AddDataAdapters(typeof(MessageCommandHandlers));
            container.Register<IMirrorRepository, MirrorRepository>();

            // TODO:1 currently using appuserid to invoke getquery of networkrepository.GetByQueryAsync
            // update IMirrorRepository so all Get methods return UserNeuronId so it can be used to initialize mirrors
            var result = Task.Run(async () => 
            {
                var queryResult = container.Resolve<INetworkRepository>().GetByQueryAsync(
                    new Library.Common.NeuronQuery()
                    {
                        PageSize = 1,
                        Page = 1
                    },
                    false
                ).Result;

                return await container.AddMirrorsAsync(
                    Common.Constants.InitMirrorKeys,
                    ss.InitializeMissingMirrors,
                    queryResult.UserNeuronId
                );
            }).Result;

            if (result.initialized)
            {
                Trace.WriteLine("Mirrors initialized successfully. Shutting down application...");
                Environment.Exit(0);
            }
            else if (result.registered)
            {

                container.Register<IClassInstanceNeuronsRetriever, ClassInstanceNeuronsRetriever>();
                container.Register<IIdInstanceNeuronsRetriever, IdInstanceNeuronsRetriever>();
                container.Register<IAvatarReadRepository, HttpAvatarReadRepository>();
                container.Register<INetworkDictionary<string>>(new NetworkDictionary<string>());
                container.Register<Id23neurULizerOptions, neurULizerOptions>();
                container.Register<IneurULizer, neurULizer>();
                container.Register<IStringWrapperWriteRepository, StringWrapperWriteRepository>();
                container.Register<ICreationWriteRepository, CreationWriteRepository>();
                container.Register<IMessageWriteRepository, HttpMessageWriteRepository>();
                container.Register<ICommunicatorWriteRepository<Sender>, HttpCommunicatorWriteRepository<Sender>>();
                container.Register<ICommunicatorWriteRepository<Recipient>, HttpCommunicatorWriteRepository<Recipient>>();
                container.Register<IWriteCacheService, WriteCacheService>();
                container.Register<MessageCommandHandlers>();
                container.Register<IAvatarWriteRepository, HttpAvatarWriteRepository>();
                container.Register<AvatarCommandHandlers>();
                container.AddWriters();
                container.AddReaders();
            }
        }
    }
}
