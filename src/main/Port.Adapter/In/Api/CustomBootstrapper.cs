using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Routing;
using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.IdentityAccess.Client.In;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Library.Client.Out;
using ei8.EventSourcing.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using neurUL.Cortex.Port.Adapter.In.InProcess;
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

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            // TODO: remove when Authorities is removed from settings
            container.Register(this.configuration);
            container.Register(this.serviceProvider.GetService<IHttpClientFactory>());

            // create a singleton instance which will be reused for all calls in current request
            var ipb = new Router();
            container.Register<ICommandSender, Router>(ipb);
            container.Register<IHandlerRegistrar, Router>(ipb);
            container.Register<IRequestProvider>(
                (tic, npo) =>
                {
                    var rp = new RequestProvider();
                    rp.SetHttpClientHandler(new HttpClientHandler());
                    return rp;
                });

            container.Register<INeuronQueryClient, HttpNeuronQueryClient>();
            container.Register<ISettingsService, SettingsService>();
            container.Register(this.serviceProvider.GetService<IOptions<List<ExternalReference>>>());
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<INeuronQueryClient, HttpNeuronQueryClient>();
            container.Register<IAvatarReadRepository, HttpAvatarReadRepository>();
            container.Register<IPermitClient, HttpPermitClient>();
            container.Register<IRecipientWriteRepository, HttpRecipientWriteRepository>();
            container.Register<INeuronService, NeuronService>();
            container.Register<ITerminalService, TerminalService>();

            // data
            container.Register<IEventStoreUrlService>(
                (tic, npo) =>
                {
                    var ss = container.Resolve<ISettingsService>();
                    return new EventStoreUrlService(
                                    ss.EventSourcingInBaseUrl + "/",
                                    ss.EventSourcingOutBaseUrl + "/"
                                    );
                });
            container.Register<IEventSerializer, EventSerializer>();

            container.Register<IAuthoredEventStore, HttpEventStoreClient>();
            container.Register<IInMemoryAuthoredEventStore, InMemoryEventStore>();
            container.Register<IRepository>((tic, npo) => new Repository(container.Resolve<IInMemoryAuthoredEventStore>()));
            container.Register<ISession, Session>();
            container.Register<ITransaction, Transaction>();

            #region Neuron
            // neuron
            container.Register<INeuronAdapter, NeuronAdapter>();
            container.Register((tic, npo) => new neurUL.Cortex.Application.Neurons.NeuronCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(), 
                container.Resolve<ISession>()
                ));
            // tag
            container.Register<ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.Tag.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.Tag.Application.ItemCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(), 
                container.Resolve<ISession>()
                ));
            // aggregate
            container.Register<ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.Aggregate.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.Aggregate.Application.ItemCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(), 
                container.Resolve<ISession>()
                ));
            // external reference
            container.Register<ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.ExternalReference.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.ExternalReference.Application.ItemCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(), 
                container.Resolve<ISession>()
                ));
            #endregion

            #region Terminal
            container.Register<ITerminalAdapter, TerminalAdapter>();
            container.Register((tic, npo) => new neurUL.Cortex.Application.Neurons.TerminalCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(), 
                container.Resolve<ISession>()
                ));
            #endregion

            container.Register<IMessageWriteRepository, HttpMessageWriteRepository>();
            container.Register<MessageCommandHandlers>();

            var ticl = new TinyIoCServiceLocator(container);
            container.Register<IServiceProvider, TinyIoCServiceLocator>(ticl);
            var registrar = new RouteRegistrar(ticl);
            registrar.Register(typeof(MessageCommandHandlers));
            // neuron - only one type from an assembly is needed to register all handlers
            registrar.Register(typeof(neurUL.Cortex.Application.Neurons.NeuronCommandHandlers));
            // tag
            registrar.Register(typeof(ei8.Data.Tag.Application.ItemCommandHandlers));
            // aggregate
            registrar.Register(typeof(ei8.Data.Aggregate.Application.ItemCommandHandlers));
            // external reference
            registrar.Register(typeof(ei8.Data.ExternalReference.Application.ItemCommandHandlers));

            ((TinyIoCServiceLocator)container.Resolve<IServiceProvider>()).SetRequestContainer(container);
        }
    }
}
