using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Routing;
using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Client.In;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.IdentityAccess.Client.In;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Library.Client.Out;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.Out;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private const string NeuronTransaction = "neuronTransaction";
        private const string TerminalTransaction = "terminalTransaction";

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
            container.Register(this.configuration);
            container.Register<ISettingsService, SettingsService>();
            container.Register<IIdentityService, IdentityService>();
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<INeuronQueryClient, HttpNeuronQueryClient>();
            container.Register<IAvatarReadRepository, HttpAvatarReadRepository>();
            container.Register(this.serviceProvider.GetService<IHttpClientFactory>());
            container.Register<IPermitClient, HttpPermitClient>();
            container.Register<IRecipientWriteRepository, HttpRecipientWriteRepository>();
            container.Register<ILibraryService, LibraryService>();
            // TODO: is this needed?
            // DEL: container.Register<INotificationClient, HttpNotificationClient>();

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

            #region NeuronTransaction
            CustomBootstrapper.CreateTransactionRegistrations(container, CustomBootstrapper.NeuronTransaction);

            // neuron
            container.Register<INeuronAdapter, NeuronAdapter>();
            container.Register((tic, npo) => new neurUL.Cortex.Application.Neurons.NeuronCommandHandlers(
                container.Resolve<IInMemoryAuthoredEventStore>(CustomBootstrapper.NeuronTransaction), 
                container.Resolve<ISession>(CustomBootstrapper.NeuronTransaction)
                ));
            // tag
            container.Register<ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.Tag.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.Tag.Application.ItemCommandHandlers(container.Resolve<IInMemoryAuthoredEventStore>(CustomBootstrapper.NeuronTransaction), container.Resolve<ISession>(CustomBootstrapper.NeuronTransaction)));
            // aggregate
            container.Register<ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.Aggregate.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.Aggregate.Application.ItemCommandHandlers(container.Resolve<IInMemoryAuthoredEventStore>(CustomBootstrapper.NeuronTransaction), container.Resolve<ISession>(CustomBootstrapper.NeuronTransaction)));
            // external reference
            container.Register<ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.ExternalReference.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.ExternalReference.Application.ItemCommandHandlers(container.Resolve<IInMemoryAuthoredEventStore>(CustomBootstrapper.NeuronTransaction), container.Resolve<ISession>(CustomBootstrapper.NeuronTransaction)));
            #endregion

            #region TerminalTransaction
            CustomBootstrapper.CreateTransactionRegistrations(container, CustomBootstrapper.TerminalTransaction);

            container.Register<ITerminalAdapter, TerminalAdapter>();
            container.Register((tic, npo) => new neurUL.Cortex.Application.Neurons.TerminalCommandHandlers(container.Resolve<IInMemoryAuthoredEventStore>(CustomBootstrapper.TerminalTransaction), container.Resolve<ISession>(CustomBootstrapper.TerminalTransaction)));
            #endregion

            container.Register<IMessageWriteRepository>((tic, npo) => new HttpMessageWriteRepository(
                container.Resolve<ITransaction>(CustomBootstrapper.NeuronTransaction),
                container.Resolve<ITransaction>(CustomBootstrapper.TerminalTransaction),
                container.Resolve<IAuthoredEventStore>(CustomBootstrapper.NeuronTransaction),
                container.Resolve<IInMemoryAuthoredEventStore>(CustomBootstrapper.NeuronTransaction),
                container.Resolve<INeuronAdapter>(),
                container.Resolve<ITerminalAdapter>(),
                container.Resolve<ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter>(),
                container.Resolve<ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter>(),
                container.Resolve<ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter>(),
                container.Resolve<ILibraryService>()
                ));
            container.Register((tic, npo) => new MessageCommandHandlers(
                container.Resolve<ITransaction>(CustomBootstrapper.NeuronTransaction),
                container.Resolve<ITransaction>(CustomBootstrapper.TerminalTransaction),
                container.Resolve<IMessageWriteRepository>(),
                container.Resolve<IRecipientWriteRepository>(),
                container.Resolve<IValidationClient>(),
                container.Resolve<ISettingsService>(),
                container.Resolve<IIdentityService>()
                ));

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

        private static void CreateTransactionRegistrations(TinyIoCContainer container, string transactionName)
        {
            container.Register<IAuthoredEventStore, HttpEventStoreClient>(transactionName);
            container.Register<IInMemoryAuthoredEventStore, InMemoryEventStore>(transactionName);
            container.Register<IRepository>((tic, npo) => new Repository(container.Resolve<IInMemoryAuthoredEventStore>(transactionName)), transactionName);
            container.Register<ISession>((tic, npo) => new Session(container.Resolve<IRepository>(transactionName)), transactionName);
            container.Register<ITransaction>(new Transaction(container.Resolve<IAuthoredEventStore>(transactionName), container.Resolve<IInMemoryAuthoredEventStore>(transactionName)), transactionName);
        }
    }
}
