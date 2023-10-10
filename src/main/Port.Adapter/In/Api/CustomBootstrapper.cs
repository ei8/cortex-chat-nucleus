using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Routing;
using ei8.Cortex.Chat.Nucleus.Application;
// TODO: using ei8.Cortex.Chat.Nucleus.Application.Access;
using ei8.Cortex.Chat.Nucleus.Application.Messages;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
// TODO: using ei8.Cortex.Chat.Nucleus.Application.Subscriptions;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.Graph.Client;
using ei8.Cortex.IdentityAccess.Client.In;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Subscriptions.Client.In;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.Out;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Net.Http;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.In.Api
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        public CustomBootstrapper()
        {
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
            container.Register<ISettingsService, SettingsService>();
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<IAccessRequestClient, HttpAccessRequestClient>();
            container.Register<INeuronGraphQueryClient, HttpNeuronGraphQueryClient>();
            container.Register<INotificationClient, HttpNotificationClient>();
            // TODO: container.Register<IAuthorClient, HttpAuthorClient>();

            // data
            container.Register<IEventStoreUrlService>(
                (tic, npo) => {
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
            container.Register<ISubscriptionsClient, HttpSubscriptionsClient>();
            // TODO: container.Register<IAccessApplicationService, AccessApplicationService>();
            // neuron
            container.Register<INeuronAdapter, NeuronAdapter>();
            container.Register((tic, npo) => new neurUL.Cortex.Application.Neurons.NeuronCommandHandlers(container.Resolve<IInMemoryAuthoredEventStore>(), container.Resolve<ISession>()));
            container.Register<ITerminalAdapter, TerminalAdapter>();
            container.Register((tic, npo) => new neurUL.Cortex.Application.Neurons.TerminalCommandHandlers(container.Resolve<IInMemoryAuthoredEventStore>(), container.Resolve<ISession>()));
            // tag
            container.Register<ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.Tag.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.Tag.Application.ItemCommandHandlers(container.Resolve<IInMemoryAuthoredEventStore>(), container.Resolve<ISession>()));
            // aggregate
            container.Register<ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.Aggregate.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.Aggregate.Application.ItemCommandHandlers(container.Resolve<IInMemoryAuthoredEventStore>(), container.Resolve<ISession>()));
            // external reference
            container.Register<ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter, ei8.Data.ExternalReference.Port.Adapter.In.InProcess.ItemAdapter>();
            container.Register((tic, npo) => new Data.ExternalReference.Application.ItemCommandHandlers(container.Resolve<IInMemoryAuthoredEventStore>(), container.Resolve<ISession>()));

            container.Register<MessageCommandHandlers>();
            // TODO: container.Register<TerminalCommandHandlers>();
            // TODO: container.Register<SubscriptionCommandHandlers>();

            var ticl = new TinyIoCServiceLocator(container);
            container.Register<IServiceProvider, TinyIoCServiceLocator>(ticl);
            var registrar = new RouteRegistrar(ticl);
            registrar.Register(typeof(MessageCommandHandlers));
            // neuron
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
