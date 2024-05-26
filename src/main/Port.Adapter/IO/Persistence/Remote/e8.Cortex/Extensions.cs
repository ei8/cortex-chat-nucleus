using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Data;
using ei8.EventSourcing.Client;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Extensions;
using neurUL.Cortex.Common;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex
{
    internal static class Extensions
    {
        public static Ensembles.Neuron CreateTransient(this INeuronService neuronService) =>
            NeuronService.CreateNeuron(
                Guid.NewGuid(),
                true
                );

        public static async Task<Ensembles.Neuron> GetExternalReference(this INeuronService neuronService, string userId, string key) =>
             (await neuronService.GetExternalReferences(userId, key)).Values.SingleOrDefault();

        public static string ToExternalReferenceKeyString(this Type value) => value.FullName;
        public static string ToExternalReferenceKeyString(this Enum value) => value.ToString();

        public static async Task<Ensembles.Neuron> GetExternalReference(
            this INeuronService neuronService,
            string userId,
            object key
            ) =>
            (await neuronService.GetExternalReferences(userId, key)).Values.SingleOrDefault();

        public static async Task<IDictionary<object, Ensembles.Neuron>> GetExternalReferences(
            this INeuronService neuronService,
            string userId,
            params object[] keys
            )
        {
            var keyConverter = new Func<object, string>(o =>
            {
                var result = o as string;
                if (o is Type)
                    result = ((Type)o).ToExternalReferenceKeyString();
                else if (o is Enum)
                    result = ((Enum)o).ToExternalReferenceKeyString();

                return result;
            });
            var origDict = await neuronService.GetExternalReferences(userId, keys.Select(t => keyConverter(t)).ToArray());
            return origDict.ToDictionary(kvpK => keys.Single(t => keyConverter(t) == kvpK.Key), kvpE => kvpE.Value);
        }

        public static async Task<Ensembles.Terminal> GetOrCreateTerminalIfNotExistsAsync(
            this ITerminalService terminalService,
            Ensembles.Neuron presynaptic,
            string userId,
            Ensembles.Neuron postsynaptic) =>
            (await terminalService.GetOrCreateTerminalsIfNotExistsAsync(presynaptic, userId, postsynaptic)).SingleOrDefault();

        #region Library.Common to EnsembleData
        public static EnsembleData ToEnsembleData(
            this Library.Common.QueryResult<Library.Common.Neuron> queryResult,
            params Library.Common.QueryResult<Library.Common.Neuron>[] queryResults)
        {
            var allNs = queryResult.Items
                .SelectMany(n => n.Traversals.SelectMany(t => t.Neurons));
            var allTs = queryResult.Items
                .SelectMany(n => n.Traversals.SelectMany(t => t.Terminals));

            foreach (var qr in queryResults)
            {
                var ns = qr.Items
                    .SelectMany(n => n.Traversals.SelectMany(t => t.Neurons));
                var ts = qr.Items
                    .SelectMany(n => n.Traversals.SelectMany(t => t.Terminals));

                allNs = allNs.Concat(ns);
                allTs = allTs.Concat(ts);
            }

            var eNs = allNs.DistinctBy(n => n.Id)
                .Select(n => n.ToEnsembleData()).ToArray();
            var eTs = allTs.DistinctBy(t => t.Id)
                .Select(t => t.ToEnsembleData()).ToArray();

            return new EnsembleData(eNs, eTs);
        }

        public static Ensembles.Data.NeuronData ToEnsembleData(this Library.Common.Neuron value)
        {
            Guid? g = null;

            if (Guid.TryParse(value.Region?.Id, out Guid gr))
                g = gr;

            return new Ensembles.Data.NeuronData()
            {
                Id = Guid.Parse(value.Id),
                Tag = value.Tag,
                ExternalReferenceUrl = value.ExternalReferenceUrl,
                RegionId = g
            };
        }

        public static TerminalData ToEnsembleData(this Library.Common.Terminal value)
        {
            var result = new TerminalData();
            result.Id = Guid.Parse(value.Id);
            result.PresynapticNeuronId = Guid.Parse(value.PresynapticNeuronId);
            result.PostsynapticNeuronId = Guid.Parse(value.PostsynapticNeuronId);
            result.Effect = Enum.TryParse(value.Effect, out NeurotransmitterEffect ne) ? ne : NeurotransmitterEffect.Excite;
            result.Strength = float.Parse(value.Strength);
            return result;
        }
        #endregion

        #region EnsembleData to Ensemble
        public static Ensembles.Neuron ToEnsemble(this EnsembleData value, Guid? resultId = null) {
            var eNs = value.Neurons.DistinctBy(n => n.Id)
                .Select(n => n.ToEnsemble()).ToList();
            var eTs = value.Terminals.DistinctBy(t => t.Id)
                .Select(t => t.ToEnsemble(eNs)).ToList();
            return resultId.HasValue ? eNs.Single(n => n.Id == resultId) : eNs[0];
        }
        
        public static Ensembles.Neuron ToEnsemble(
            this Ensembles.Data.NeuronData value
            ) => NeuronService.CreateNeuron(
                value.Id,
                false,
                value.Tag,
                value.ExternalReferenceUrl,
                value.RegionId
                );

        public static Ensembles.Terminal ToEnsemble(
            this TerminalData value,
            IEnumerable<Ensembles.Neuron> neurons
        )
        {
            var presynaptic = neurons.Single(n => n.Id.ToString() == value.PresynapticNeuronId.ToString());
            var postsynaptic = neurons.Single(n => n.Id.ToString() == value.PostsynapticNeuronId.ToString());

            var result = new Ensembles.Terminal();
            result.Id = value.Id;
            result.IsTransient = false;
            result.Strength = value.Strength;
            result.Effect = value.Effect;

            result.Presynaptic = presynaptic;
            result.Postsynaptic = postsynaptic;
            presynaptic.AddTerminal(result);
            postsynaptic.AddDendrite(result);

            return result;
        }
        #endregion

        #region Library.Common to Ensemble
        public static Ensembles.Neuron ToEnsemble(
            this Library.Common.QueryResult<Library.Common.Neuron> queryResult,
            params Library.Common.QueryResult<Library.Common.Neuron>[] queryResults)
        {
            var allNs = queryResult.Items
                .SelectMany(n => n.Traversals.SelectMany(t => t.Neurons));
            var allTs = queryResult.Items
                .SelectMany(n => n.Traversals.SelectMany(t => t.Terminals));

            foreach (var qr in queryResults)
            {
                var ns = qr.Items
                    .SelectMany(n => n.Traversals.SelectMany(t => t.Neurons));
                var ts = qr.Items
                    .SelectMany(n => n.Traversals.SelectMany(t => t.Terminals));

                allNs = allNs.Concat(ns);
                allTs = allTs.Concat(ts);
            }

            var eNs = allNs.DistinctBy(n => n.Id)
                .Select(n => n.ToEnsemble()).ToList();
            var eTs = allTs.DistinctBy(t => t.Id)
                .Select(t => t.ToEnsemble(eNs)).ToList();

            return eNs[0];
        }

        public static Ensembles.Neuron ToEnsemble(
            this Library.Common.Neuron value
            ) 
        {
            Guid? g = null;

            if (Guid.TryParse(value.Region?.Id, out Guid gr))
                g = gr;
            
            return NeuronService.CreateNeuron(
                Guid.Parse(value.Id),
                false,
                value.Tag,
                value.ExternalReferenceUrl,
                g   
            );
        }

        public static Ensembles.Terminal ToEnsemble(
            this Library.Common.Terminal value,
            IEnumerable<Ensembles.Neuron> neurons
        )
        {
            var presynaptic = neurons.Single(n => n.Id.ToString() == value.PresynapticNeuronId);
            var postsynaptic = neurons.Single(n => n.Id.ToString() == value.PostsynapticNeuronId);

            var result = new Ensembles.Terminal();
            result.Id = Guid.Parse(value.Id);
            result.IsTransient = false;
            result.Strength = float.Parse(value.Strength);
            result.Effect = Enum.TryParse(value.Effect, out NeurotransmitterEffect ne) ? ne : NeurotransmitterEffect.Excite;

            result.Presynaptic = presynaptic;
            result.Postsynaptic = postsynaptic;
            presynaptic.AddTerminal(result);
            postsynaptic.AddDendrite(result);

            return result;
        }
        #endregion

        #region Ensemble to EnsembleData
        public static EnsembleData ToEnsembleData(this Ensembles.Neuron neuron, bool includeTransientOnly = true)
        {
            var neuronsList = new List<Ensembles.Data.NeuronData>();
            var terminalsList = new List<TerminalData>();

            ExtractData(neuron, neuronsList, terminalsList, includeTransientOnly);

            return new EnsembleData(neuronsList.ToArray(), terminalsList.ToArray());
        }
        #endregion

        private static void ExtractData(Ensembles.Neuron neuron, List<Ensembles.Data.NeuronData> neuronsList, List<TerminalData> terminalsList, bool includeTransientOnly)
        {
            if (!includeTransientOnly || neuron.IsTransient) 
                neuronsList.Add(
                    new Ensembles.Data.NeuronData()
                    {
                        Id = neuron.Id,
                        Tag = neuron.Tag,
                        ExternalReferenceUrl = neuron.ExternalReferenceUrl,
                        RegionId = neuron.RegionId
                    });

            neuron.Terminals.ToList().ForEach(t => {
                ExtractData(t.Postsynaptic, neuronsList, terminalsList, includeTransientOnly);

                if (!includeTransientOnly || t.IsTransient)
                    terminalsList.Add(
                        new TerminalData()
                        {
                            Id = t.Id,
                            Effect = t.Effect,
                            PresynapticNeuronId = t.Presynaptic.Id,
                            PostsynapticNeuronId =  t.Postsynaptic.Id,
                            Strength = t.Strength
                        });
            });
        }

        public static async Task SaveEnsembleDataAsync(
           this ITransaction transaction,
           IServiceProvider serviceProvider,
           EnsembleData ensembleData,
           Guid authorId
           )
        {
            foreach(var n in ensembleData.Neurons)
                await transaction.SaveNeuronDataAsync(serviceProvider, n, authorId);
            foreach(var t in ensembleData.Terminals)
                await transaction.SaveTerminalDataAsync(serviceProvider, t, authorId);
        }

        public static async Task SaveTerminalDataAsync(
           this ITransaction transaction,
           IServiceProvider serviceProvider,
           TerminalData terminalData,
           Guid authorId
           )
        {
            var terminalAdapter = serviceProvider.GetRequiredService<ITerminalAdapter>();

            await transaction.InvokeAdapterAsync(
                terminalData.Id,
                typeof(TerminalCreated).Assembly.GetEventTypes(),
                async (ev) => await terminalAdapter.CreateTerminal(
                    terminalData.Id,
                    terminalData.PresynapticNeuronId,
                    terminalData.PostsynapticNeuronId,
                    terminalData.Effect,
                    terminalData.Strength,
                    authorId
                )
            );
        }

        public static async Task SaveNeuronDataAsync(
           this ITransaction transaction,
           IServiceProvider serviceProvider,
           Ensembles.Data.NeuronData neuronData,
           Guid authorId
           )
        {
            var neuronAdapter = serviceProvider.GetRequiredService<INeuronAdapter>();
            var tagItemAdapter = serviceProvider.GetRequiredService<Data.Tag.Port.Adapter.In.InProcess.IItemAdapter>();
            var aggregateItemAdapter = serviceProvider.GetRequiredService<Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter>();
            var externalReferenceItemAdapter = serviceProvider.GetRequiredService<Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter>();

            #region Create instance neuron
            int expectedVersion = await transaction.InvokeAdapterAsync(
                    neuronData.Id,
                    typeof(NeuronCreated).Assembly.GetEventTypes(),
                    async (ev) => await neuronAdapter.CreateNeuron(
                        neuronData.Id,
                        authorId)
                    );

            // assign tag value
            expectedVersion = await transaction.InvokeAdapterAsync(
                neuronData.Id,
                typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly.GetEventTypes(),
                async (ev) => await tagItemAdapter.ChangeTag(
                    neuronData.Id,
                    neuronData.Tag,
                    authorId,
                    ev
                ),
                expectedVersion
                );

            if (neuronData.RegionId.HasValue)
            {
                // assign region value to id
                expectedVersion = await transaction.InvokeAdapterAsync(
                    neuronData.Id,
                    typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly.GetEventTypes(),
                    async (ev) => await aggregateItemAdapter.ChangeAggregate(
                        neuronData.Id,
                        neuronData.RegionId.ToString(),
                        authorId,
                        ev
                    ),
                    expectedVersion
                );
            }

            if (!string.IsNullOrWhiteSpace(neuronData.ExternalReferenceUrl))
            {
                expectedVersion = await transaction.InvokeAdapterAsync(
                    neuronData.Id,
                    typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly.GetEventTypes(),
                    async (ev) => await externalReferenceItemAdapter.ChangeUrl(
                        neuronData.Id,
                        neuronData.ExternalReferenceUrl,
                        authorId,
                        ev
                    ),
                    expectedVersion
                    );
            }
            #endregion
        }

        public static bool HasSameElementsAs<T>(
                this IEnumerable<T> first,
                IEnumerable<T> second
            )
        {
            var firstMap = first
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
            var secondMap = second
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
            return
                firstMap.Keys.All(x =>
                    secondMap.Keys.Contains(x) && firstMap[x] == secondMap[x]
                ) &&
                secondMap.Keys.All(x =>
                    firstMap.Keys.Contains(x) && secondMap[x] == firstMap[x]
                );
        }
    }
}
