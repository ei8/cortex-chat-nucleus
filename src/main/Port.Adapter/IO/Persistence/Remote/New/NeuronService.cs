using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using Microsoft.Extensions.Options;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Application.Neurons.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
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

        public async Task<IDictionary<string, Neuron>> GetExternalReferences(string userId, params string[] keys)
        {
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
            var result = new Dictionary<string, Neuron>();

            foreach (var n in qr.Items)
            {
                Guid? r = null;
                if (Guid.TryParse(n.Region?.Id, out Guid g))
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

        internal static Neuron CreateNeuron(Guid id, bool isTransient, string tag = null, string externalReferenceUrl = null, Guid? regionId = null)
        {
            var result = new New.Neuron();
            result.Id = id;
            result.IsTransient = isTransient;
            result.Tag = tag;
            result.ExternalReferenceUrl = externalReferenceUrl;
            result.RegionId = regionId;
            return result;
        }
    }
}
    