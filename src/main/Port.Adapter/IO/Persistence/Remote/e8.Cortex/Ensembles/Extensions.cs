using ei8.EventSourcing.Client;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Extensions;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Common;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public static class Extensions
    {
        #region IEnsembleService
        /// <summary>
        /// Retrieves grandmother from ensemble if present; Otherwise, retrieves it from persistence or builds it, and adds it to the ensemble.
        /// </summary>
        /// <typeparam name="TEnsembleService"></typeparam>
        /// <typeparam name="TParameterSet"></typeparam>
        /// <param name="ensembleService"></param>
        /// <param name="ensemble"></param>
        /// <param name="parameterSet"></param>
        /// <param name="neuronRepository"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<Neuron> ObtainAsync<TEnsembleService, TParameterSet>(
                this IEnsembleService<TEnsembleService, TParameterSet> ensembleService, 
                Ensemble ensemble,
                TParameterSet parameterSet, 
                INeuronRepository neuronRepository, 
                string userId
            ) 
            where TEnsembleService : IEnsembleService<TEnsembleService, TParameterSet>
            where TParameterSet : IParameterSet
        {
            AssertionConcern.AssertArgumentNotNull(ensemble, nameof(ensemble));
            AssertionConcern.AssertArgumentNotNull(parameterSet, nameof(parameterSet));
            AssertionConcern.AssertArgumentNotNull(neuronRepository, nameof(neuronRepository));
            AssertionConcern.AssertArgumentNotEmpty(userId, "Specified value cannot be null or empty.", nameof(userId));

            Neuron result = null;
            // if target is not in specified ensemble
            if (!ensembleService.TryParse(ensemble, parameterSet, out Neuron ensembleParseResult))
            {
                // retrieve target from DB
                var queries = ensembleService.GetQueries(parameterSet);
                ensemble.AddReplaceItems(await neuronRepository.GetByQueriesAsync(userId, queries.ToArray()));
                // if target is in DB
                if (ensembleService.TryParse(ensemble, parameterSet, out Neuron dbParseResult))
                {
                    result = dbParseResult;
                }
                // else if target is not in DB 
                else
                {
                    // build in ensemble
                    result = await ensembleService.BuildAsync(ensemble, parameterSet, neuronRepository, userId);
                }
            }
            // if target was found in ensemble
            else if (ensembleParseResult != null)
                result = ensembleParseResult;

            return result;
        }
        #endregion

        #region INeuronRepository
        public static async Task<Ensembles.Neuron> GetExternalReferenceAsync(this INeuronRepository neuronRepository, string userId, string key) =>
             (await neuronRepository.GetExternalReferencesAsync(userId, key)).Values.SingleOrDefault();

        public static async Task<Ensembles.Neuron> GetExternalReferenceAsync(
            this INeuronRepository neuronRepository,
            string userId,
            object key
            ) =>
            (await neuronRepository.GetExternalReferencesAsync(userId, key)).Values.SingleOrDefault();

        public static async Task<IDictionary<object, Ensembles.Neuron>> GetExternalReferencesAsync(
            this INeuronRepository neuronRepository,
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
            var origDict = await neuronRepository.GetExternalReferencesAsync(userId, keys.Select(t => keyConverter(t)).ToArray());
            return origDict.ToDictionary(kvpK => keys.Single(t => keyConverter(t) == kvpK.Key), kvpE => kvpE.Value);
        }

        public static string ToExternalReferenceKeyString(this Type value) => value.FullName;
        public static string ToExternalReferenceKeyString(this Enum value) => value.ToString();
        #endregion

        #region ITerminalRepository
        public static async Task<Ensembles.Terminal> ObtainLinkAsync(
            this ITerminalService terminalService,
            Ensembles.Neuron presynaptic,
            string userId,
            Ensembles.Neuron postsynaptic) =>
            (await terminalService.ObtainLinkAsync(presynaptic, userId, postsynaptic)).SingleOrDefault();
        #endregion

        #region ITransaction
        public static async Task SaveEnsembleAsync(
           this ITransaction transaction,
           IServiceProvider serviceProvider,
           Ensemble ensemble,
           Guid authorId
           )
        {
            foreach (var ei in ensemble.GetItems<IEnsembleItem>().Where(ei => ei.IsTransient))
                await transaction.SaveItemAsync(serviceProvider, ei, authorId);
        }

        public static async Task SaveItemAsync(
           this ITransaction transaction,
           IServiceProvider serviceProvider,
           IEnsembleItem item,
           Guid authorId
           )
        {
            if (item is Terminal terminal)
            {
                var terminalAdapter = serviceProvider.GetRequiredService<ITerminalAdapter>();

                await transaction.InvokeAdapterAsync(
                    terminal.Id,
                    typeof(TerminalCreated).Assembly.GetEventTypes(),
                    async (ev) => await terminalAdapter.CreateTerminal(
                        terminal.Id,
                        terminal.PresynapticNeuronId,
                        terminal.PostsynapticNeuronId,
                        terminal.Effect,
                        terminal.Strength,
                        authorId
                    )
                );
            }
            else if (item is Neuron neuron)
            {
                var neuronAdapter = serviceProvider.GetRequiredService<INeuronAdapter>();
                var tagItemAdapter = serviceProvider.GetRequiredService<Data.Tag.Port.Adapter.In.InProcess.IItemAdapter>();
                var aggregateItemAdapter = serviceProvider.GetRequiredService<Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter>();
                var externalReferenceItemAdapter = serviceProvider.GetRequiredService<Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter>();

                #region Create instance neuron
                int expectedVersion = await transaction.InvokeAdapterAsync(
                        neuron.Id,
                        typeof(NeuronCreated).Assembly.GetEventTypes(),
                        async (ev) => await neuronAdapter.CreateNeuron(
                            neuron.Id,
                            authorId)
                        );

                // assign tag value
                if (!string.IsNullOrWhiteSpace(neuron.Tag))
                {
                    expectedVersion = await transaction.InvokeAdapterAsync(
                        neuron.Id,
                        typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly.GetEventTypes(),
                        async (ev) => await tagItemAdapter.ChangeTag(
                            neuron.Id,
                            neuron.Tag,
                            authorId,
                            ev
                        ),
                        expectedVersion
                        );
                }

                if (neuron.RegionId.HasValue)
                {
                    // assign region value to id
                    expectedVersion = await transaction.InvokeAdapterAsync(
                        neuron.Id,
                        typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly.GetEventTypes(),
                        async (ev) => await aggregateItemAdapter.ChangeAggregate(
                            neuron.Id,
                            neuron.RegionId.ToString(),
                            authorId,
                            ev
                        ),
                        expectedVersion
                    );
                }

                if (!string.IsNullOrWhiteSpace(neuron.ExternalReferenceUrl))
                {
                    expectedVersion = await transaction.InvokeAdapterAsync(
                        neuron.Id,
                        typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly.GetEventTypes(),
                        async (ev) => await externalReferenceItemAdapter.ChangeUrl(
                            neuron.Id,
                            neuron.ExternalReferenceUrl,
                            authorId,
                            ev
                        ),
                        expectedVersion
                        );
                }
                #endregion
            }
        }
        #endregion

        #region Neuron
        public static IEnumerable<Terminal> GetDendrites(this Neuron neuron, Ensemble ensemble) =>
            ensemble.GetItems<Terminal>().Where(t => t.PostsynapticNeuronId == neuron.Id);

        public static IEnumerable<Terminal> GetTerminals(this Neuron neuron, Ensemble ensemble) =>
            ensemble.GetItems<Terminal>().Where(t => t.PresynapticNeuronId == neuron.Id);

        public static IEnumerable<Neuron> GetPresynapticNeurons(this Neuron neuron, Ensemble ensemble) =>
            neuron.GetDendrites(ensemble)
                 .Select(t => {
                     AssertionConcern.AssertStateTrue(
                         ensemble.TryGetById(t.PresynapticNeuronId, out Neuron result), 
                         "Neuron with specified Presynaptic Neuron Id was not found."
                         );
                     return result;
                     });
        #endregion

        #region Library.Common to Ensemble
        public static Ensemble ToEnsemble(this IEnumerable<Library.Common.QueryResult<Library.Common.Neuron>> queryResults)
        {
            var allNs = queryResults.SelectMany(qr => qr.Items.SelectMany(n => n.Traversals.SelectMany(t => t.Neurons)));
            var allTs = queryResults.SelectMany(qr => qr.Items.SelectMany(n => n.Traversals.SelectMany(t => t.Terminals)));

            var eNs = allNs.DistinctBy(n => n.Id)
                .Select(n => n.ToEnsemble());
            var eTs = allTs.DistinctBy(t => t.Id)
                .Select(t => t.ToEnsemble());

            return new Ensemble(
                eNs.Cast<IEnsembleItem>().Concat(
                    eTs.Cast<IEnsembleItem>()
                ).ToDictionary(ei => ei.Id)
            );
        }

        public static Neuron ToEnsemble(
            this Library.Common.Neuron value
            )
        {
            Guid? g = null;

            if (Guid.TryParse(value.Region?.Id, out Guid gr))
                g = gr;

            return new Neuron(
                Guid.Parse(value.Id),
                value.Tag,
                value.ExternalReferenceUrl,
                g
            );
        }

        public static Terminal ToEnsemble(
            this Library.Common.Terminal value
        )
        {
            var result = new Terminal(
                Guid.Parse(value.Id),
                Guid.Parse(value.PresynapticNeuronId),
                Guid.Parse(value.PostsynapticNeuronId),
                Enum.TryParse(value.Effect, out NeurotransmitterEffect ne) ? ne : NeurotransmitterEffect.Excite,
                float.Parse(value.Strength)
                );

            return result;
        }
        #endregion
        
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
