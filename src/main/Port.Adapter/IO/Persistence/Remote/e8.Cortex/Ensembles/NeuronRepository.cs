﻿using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using Microsoft.Extensions.Options;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class NeuronRepository : INeuronRepository
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;
        private IEnumerable<ExternalReference> externalReferences;

        public NeuronRepository(INeuronQueryClient neuronQueryClient, ISettingsService settingsService, IOptions<List<ExternalReference>> externalReferences)
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(externalReferences, nameof(externalReferences));

            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
            this.externalReferences = externalReferences.Value.ToArray();
        }

        public async Task<Ensemble> GetByQueriesAsync(string userId, params NeuronQuery[] queries)
        {
            AssertionConcern.AssertArgumentNotEmpty(userId, "Specified 'userId' cannot be null or empty.", nameof(userId));
            AssertionConcern.AssertArgumentNotNull(queries, nameof(queries));
            AssertionConcern.AssertArgumentValid(k => k.Length > 0, queries, "Specified array cannot be an empty array.", nameof(queries));

            var qrs = new List<Library.Common.QueryResult<Library.Common.Neuron>>();
            foreach (var q in queries)
            {
                var qr = await this.neuronQueryClient.GetNeuronsInternal(
                        settingsService.CortexLibraryOutBaseUrl + "/",
                        q,
                        userId
                        );

                qrs.Add(qr);
            }

            return qrs.ToEnsemble();
        }

        public async Task<IDictionary<string, Neuron>> GetExternalReferencesAsync(string userId, params string[] keys)
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
            var result = new Dictionary<string, Neuron>();

            foreach (var n in qr.Items)
            {
                Guid? r = null;
                if (Guid.TryParse(n?.Id, out Guid g))
                    r = g;
                result.Add(
                    exRefs.Single(er => er.Url == n.ExternalReferenceUrl).Key,
                    new Neuron(
                        Guid.Parse(n.Id),
                        n.Tag,
                        n.ExternalReferenceUrl,
                        r
                    )
                );
            }

            return result;
        }
    }
}