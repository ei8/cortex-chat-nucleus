﻿using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpAvatarReadRepository : IAvatarReadRepository
    {
        private readonly IExternalReferenceRepository externalReferenceRepository;
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;
        private readonly IGrannyService grannyService;

        public HttpAvatarReadRepository(
            IExternalReferenceRepository externalReferenceRepository,
            INeuronQueryClient neuronQueryClient,
            ISettingsService settingsService,
            IGrannyService grannyService
        )
        {
            AssertionConcern.AssertArgumentNotNull(externalReferenceRepository, nameof(externalReferenceRepository));
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(grannyService, nameof(grannyService));

            this.externalReferenceRepository = externalReferenceRepository;
            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
            this.grannyService = grannyService;
        }

        public async Task<IEnumerable<Avatar>> GetAll(string userId, CancellationToken token = default)
        {
            var instantiatesAvatarResult = await this.grannyService.TryGetParseBuildPersistAsync(
                new InstantiatesClassGrannyInfo(
                    new Coding.d23.neurULization.Processors.Readers.Deductive.InstantiatesClassParameterSet(
                        await this.externalReferenceRepository.GetByKeyAsync(
                            typeof(Avatar)
                        )
                    )
                ),
                token
            );

            AssertionConcern.AssertStateTrue(
                instantiatesAvatarResult.Item1,
                $"'Instantiates^Avatar' is required to invoke {nameof(HttpAvatarReadRepository.GetAll)}"
            );
                       
            var neurons = await this.neuronQueryClient.GetNeuronsInternal(
                this.settingsService.CortexLibraryOutBaseUrl + "/",
                new NeuronQuery()
                {
                    Postsynaptic = new string[] { instantiatesAvatarResult.Item2.Neuron.Id.ToString() },
                    SortBy = SortByValue.NeuronExternalReferenceUrl,
                    SortOrder = SortOrderValue.Ascending
                },
                userId
            );

            return neurons.Items.Select(n => n.ToDomainAvatar());
        }

        public async Task<IEnumerable<Avatar>> GetByIds(IEnumerable<Guid> ids, string userId, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(ids, nameof(ids));

            var result = (IEnumerable<Avatar>) Array.Empty<Avatar>();

            if (ids.Any())
            {
                var neurons = await this.neuronQueryClient.GetNeuronsInternal(
                    this.settingsService.CortexLibraryOutBaseUrl + "/",
                    new NeuronQuery()
                    {
                        Id = ids.Select(i => i.ToString()),
                        SortBy = SortByValue.NeuronExternalReferenceUrl,
                        SortOrder = SortOrderValue.Ascending
                    },
                    userId
                    );

                result = neurons.Items.Select(n => n.ToDomainAvatar());
            }

            return result;
        }
    }
}
