﻿using ei8.Cortex.Chat.Nucleus.Application;
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
using ei8.Cortex.Coding.Persistence.Wrappers;
using ei8.Cortex.IdentityAccess.Client.In;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Library.Client.Out;
using ei8.Extensions.DependencyInjection;
using ei8.Extensions.DependencyInjection.Coding.d23.neurULization.Persistence;
using ei8.Extensions.DependencyInjection.Coding.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nancy;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Net.Http;

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

            // TODO: change IDictionary<string, Ensemble> to EnsembleCache
            container.Register<IDictionary<string, Ensemble>>(new Dictionary<string, Ensemble>());
            container.Register(this.serviceProvider.GetService<IOptions<List<ExternalReference>>>());
            container.Register(this.serviceProvider.GetService<IOptions<List<Authority>>>());
            container.Register<ISettingsService, SettingsService>();

            var ss = container.Resolve<ISettingsService>();
            container.AddExternalReferences(
                ExternalReferenceRepository.CreateTransient(
                    this.serviceProvider.GetService<IHttpClientFactory>(),
                    ss.EventSourcingInBaseUrl + "/",
                    ss.EventSourcingOutBaseUrl + "/",
                    ss.CortexLibraryOutBaseUrl + "/",
                    ss.IdentityAccessOutBaseUrl + "/",
                    container.Resolve<IOptions<List<ExternalReference>>>(),
                    ss.AppUserId
                ),
                Default.ExternalReferenceKeys,
                ss.CreateExternalReferencesIfNotFound
            ).Wait();
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.AddRequestProvider();
            container.Register(this.serviceProvider.GetService<IHttpClientFactory>());
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<IAvatarReadRepository, HttpAvatarReadRepository>();
            container.Register<IPermitClient, HttpPermitClient>();
            container.Register<IRecipientWriteRepository, HttpRecipientWriteRepository>();
            container.Register<IEnsembleTransactionData, EnsembleTransactionData>();
            container.Register<IEnsembleTransactionService, EnsembleTransactionService>();
            container.Register<INeuronQueryClient, HttpNeuronQueryClient>();
            var ss = container.Resolve<ISettingsService>();
            container.AddExternalReferenceRepository(
                ss.EventSourcingInBaseUrl + "/",
                ss.EventSourcingOutBaseUrl + "/",
                ss.CortexLibraryOutBaseUrl + "/",
                ss.IdentityAccessOutBaseUrl + "/",
                ss.AppUserId
            );
            container.AddEnsembleRepository(ss.CortexLibraryOutBaseUrl + "/", ss.QueryResultLimit, ss.AppUserId);
            container.AddGrannyService(ss.IdentityAccessOutBaseUrl + "/", ss.AppUserId);
            container.Register<Id23neurULizerOptions, neurULizerOptions>();
            container.Register<IneurULizer, neurULizer>();
            container.Register<IStringWrapperRepository, StringWrapperRepository>();
            container.Register<IMessageWriteRepository, HttpMessageWriteRepository>();
            container.Register<MessageCommandHandlers>();
            container.AddTransactions(ss.EventSourcingInBaseUrl + "/", ss.EventSourcingOutBaseUrl + "/");
            container.AddWriters();
            container.AddReaders();
            container.AddDataAdapters(typeof(MessageCommandHandlers));
        }
    }
}
