using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using Microsoft.Extensions.Options;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class EnsembleRepository : IEnsembleRepository
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;
        private IEnumerable<ExternalReference> externalReferences;

        public EnsembleRepository(INeuronQueryClient neuronQueryClient, ISettingsService settingsService, IOptions<List<ExternalReference>> externalReferences)
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(externalReferences, nameof(externalReferences));

            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
            this.externalReferences = externalReferences.Value.ToArray();
        }

        public async Task<Ensemble> GetByQueryAsync(string userId, NeuronQuery query)
        {
            AssertionConcern.AssertArgumentNotEmpty(userId, "Specified 'userId' cannot be null or empty.", nameof(userId));
            AssertionConcern.AssertArgumentNotNull(query, nameof(query));

            var qr = await neuronQueryClient.GetNeuronsInternal(
                    settingsService.CortexLibraryOutBaseUrl + "/",
                    query,
                    userId
                    );

            return qr.ToEnsemble();
        }

        public async Task<IDictionary<string, Coding.Neuron>> GetExternalReferencesAsync(string userId, params string[] keys)
        {
            AssertionConcern.AssertArgumentNotEmpty(userId, "Specified 'userId' cannot be null or empty.", nameof(userId));
            AssertionConcern.AssertArgumentNotNull(keys, nameof(keys));
            AssertionConcern.AssertArgumentValid(k => k.Length > 0, keys, "Specified array cannot be an empty array.", nameof(keys));

            var exRefs = externalReferences.Where(er => keys.Contains(er.Key));
            var qr = await neuronQueryClient.GetNeuronsInternal(
                    settingsService.CortexLibraryOutBaseUrl + "/",
                    new NeuronQuery()
                    {
                        ExternalReferenceUrl = exRefs.Select(er => er.Url),
                        SortBy = SortByValue.NeuronCreationTimestamp,
                        SortOrder = SortOrderValue.Descending,
                        PageSize = exRefs.Count()
                    },
                    userId
                );
            AssertionConcern.AssertStateTrue(keys.Length == qr.Count, "At least one local copy of a specified External Reference was not found.");
            var result = new Dictionary<string, Coding.Neuron>();

            foreach (var n in qr.Items)
            {
                Guid? r = null;
                if (Guid.TryParse(n?.Id, out Guid g))
                    r = g;
                result.Add(
                    exRefs.Single(er => er.Url == n.ExternalReferenceUrl).Key,
                    n.ToEnsemble()
                );
            }

            return result;
        }
    }
}