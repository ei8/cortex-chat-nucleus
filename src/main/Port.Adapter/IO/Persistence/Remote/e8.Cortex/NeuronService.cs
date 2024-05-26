using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using Microsoft.Extensions.Options;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex
{
    public class NeuronService : INeuronService
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;
        private IEnumerable<ExternalReference> externalReferences;

        public NeuronService(INeuronQueryClient neuronQueryClient, ISettingsService settingsService, IOptions<List<ExternalReference>> externalReferences)
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(externalReferences, nameof(externalReferences));

            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
            this.externalReferences = externalReferences.Value.ToArray();
        }

        public async Task<IDictionary<string, Ensembles.Neuron>> GetExternalReferences(string userId, params string[] keys)
        {
            AssertionConcern.AssertArgumentNotEmpty(userId, "Specified 'userId' cannot be null or empty.", nameof(userId));
            AssertionConcern.AssertArgumentNotNull(keys, nameof(keys));
            AssertionConcern.AssertArgumentValid(k => k.Length > 0, keys, "Specified 'keys' array cannot be an empty array.", nameof(keys));
            
            var exRefs = this.externalReferences.Where(er => keys.Contains(er.Key));
            var qr = (await this.neuronQueryClient.GetNeuronsInternal(
                    this.settingsService.CortexLibraryOutBaseUrl + "/",
                    new NeuronQuery()
                    {
                        ExternalReferenceUrl = exRefs.Select(er => er.Url),
                        SortBy = SortByValue.NeuronCreationTimestamp,
                        SortOrder = SortOrderValue.Descending,
                        PageSize = exRefs.Count()
                    },
                    userId
                ));
            AssertionConcern.AssertStateTrue(keys.Length == qr.Count, "At least one local copy of a specified External Reference was not found.");
            var result = new Dictionary<string, Ensembles.Neuron>();

            foreach (var n in qr.Items)
            {
                Guid? r = null;
                if (Guid.TryParse(n?.Id, out Guid g))
                    r = g;
                result.Add(
                    exRefs.Single(er => er.Url == n.ExternalReferenceUrl).Key,
                    NeuronService.CreateNeuron(
                        Guid.Parse(n.Id),
                        false,
                        n.Tag,
                        n.ExternalReferenceUrl,
                        r
                    )
                );
            }

            return result;
        }

        internal static Ensembles.Neuron CreateNeuron(Guid id, bool isTransient, string tag = null, string externalReferenceUrl = null, Guid? regionId = null)
        {
            var result = new Ensembles.Neuron();
            result.Id = id;
            result.IsTransient = isTransient;
            result.Tag = tag;
            result.ExternalReferenceUrl = externalReferenceUrl;
            result.RegionId = regionId;
            return result;
        }

        public async Task<IDictionary<string, Ensembles.Neuron>> GetInstantiatesClassNeurons(string userId, params string[] keys)
        {
            AssertionConcern.AssertArgumentNotEmpty(userId, "Specified 'userId' cannot be null or empty.", nameof(userId));
            AssertionConcern.AssertArgumentNotNull(keys, nameof(keys));
            AssertionConcern.AssertArgumentValid(k => k.Length > 0, keys, "Specified 'keys' array cannot be an empty array.", nameof(keys));

            var result = new Dictionary<string, Ensembles.Neuron>();

            var coreNeuronKeys = new object[]
                {
                    ExternalReferenceKey.DirectObject,
                    ExternalReferenceKey.Subordination,
                    ExternalReferenceKey.Instantiates_Unit
                };
            var neuronCache = await this.GetExternalReferences(userId, coreNeuronKeys.Concat(keys).ToArray());

            AssertionConcern.AssertStateTrue(keys.Length + coreNeuronKeys.Length == neuronCache.Count(), "At least one local copy of a specified External Reference was not found.");

            foreach (var k in keys)
            {
                var icnip = new Ensembles.InstantiatesClass.IdProvider(
                        neuronCache[k].Id,
                        neuronCache[ExternalReferenceKey.DirectObject].Id,
                        neuronCache[ExternalReferenceKey.Subordination].Id,
                        neuronCache[ExternalReferenceKey.Instantiates_Unit].Id
                    );

                var ic = new InstantiatesClass();
                var queries = ic.GetDataQueries(icnip);
                var classQr = await this.neuronQueryClient.GetNeuronsInternal(
                        this.settingsService.CortexLibraryOutBaseUrl + "/",
                        queries.Single(),
                        userId
                    );

                var ens = classQr.ToEnsembleData();

                if (ic.TryParse(ens, icnip, out InstantiatesClass n))
                    result.Add(k, n.Neuron);
            }
            
            return result;
        }
    }
}