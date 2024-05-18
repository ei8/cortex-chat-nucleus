using ei8.Cortex.Chat.Nucleus.Application;
using ei8.EventSourcing.Client;
using Microsoft.Extensions.DependencyInjection;
using neurUL.Cortex.Application.Neurons.Commands;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static SQLite.SQLite3;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    internal static class Extensions
    {
        public static Neuron CreateTransient(this INeuronService neuronService) =>
            NeuronService.CreateNeuron(
                Guid.NewGuid(),
                true
                );

        public static async Task<Neuron> GetExternalReference(this INeuronService neuronService, string userId, string key) =>
             (await neuronService.GetExternalReferences(userId, key)).Values.SingleOrDefault();

        public static string ToExternalReferenceKeyString(this Type value) => value.FullName;
        public static string ToExternalReferenceKeyString(this Enum value) => value.ToString();

        public static async Task<New.Neuron> GetExternalReference(
            this INeuronService neuronService,
            string userId,
            object key
            ) =>
            (await neuronService.GetExternalReferences(userId, key)).Values.SingleOrDefault();

        public static async Task<IDictionary<object, New.Neuron>> GetExternalReferences(
            this INeuronService neuronService,
            string userId,
            params object[] keys
            )
        {
            var keyConverter = new Func<object, string>(o =>
            {
                var result = string.Empty;
                if (o is Type)
                    result = ((Type)o).ToExternalReferenceKeyString();
                else if (o is Enum)
                    result = ((Enum)o).ToExternalReferenceKeyString();

                return result;
            });
            var origDict = await neuronService.GetExternalReferences(userId, keys.Select(t => keyConverter(t)).ToArray());
            return origDict.ToDictionary(kvpK => keys.Single(t => keyConverter(t) == kvpK.Key), kvpE => kvpE.Value);
        }

        public static async Task<Terminal> GetOrCreateTerminalIfNotExistsAsync(
            this ITerminalService terminalService,
            Neuron presynaptic,
            string userId,
            Neuron postsynaptic) =>
            (await terminalService.GetOrCreateTerminalsIfNotExistsAsync(presynaptic, userId, postsynaptic)).SingleOrDefault();

        public static New.Neuron CreateFromData(
            this INeuronService neuronService,
            NeuronData value
            ) => NeuronService.CreateNeuron(
                value.Id,
                false,
                value.Tag,
                value.ExternalReferenceUrl,
                value.RegionId
                );

        public static EnsembleData ToSerializableData(this Neuron neuron, bool includeTransientOnly = true)
        {
            var neuronsList = new List<NeuronData>();
            var terminalsList = new List<TerminalData>();

            ExtractData(neuron, neuronsList, terminalsList, includeTransientOnly);

            return new EnsembleData(neuronsList.ToArray(), terminalsList.ToArray());
        }

        private static void ExtractData(Neuron neuron, List<NeuronData> neuronsList, List<TerminalData> terminalsList, bool includeTransientOnly)
        {
            if (!includeTransientOnly || neuron.IsTransient) 
                neuronsList.Add(
                    new NeuronData()
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
           NeuronData neuronData,
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

    }
}
