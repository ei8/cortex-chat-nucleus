using Nancy.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public static class Extensions
    {
        #region IEnsembleService
        public async static Task<Neuron> ObtainAsync<TEnsembleService, TParameterSet>(
                this IEnsembleService<TEnsembleService, TParameterSet> ensembleService,
                TParameterSet parameterSet,
                INeuronRepository neuronRepository,
                string userId
            )
            where TEnsembleService : IEnsembleService<TEnsembleService, TParameterSet>
            where TParameterSet : IParameterSet
            => await ensembleService.ObtainAsync(null, null, parameterSet, neuronRepository, userId);

        /// <summary>
        /// Use to:
        /// 1. Create ensembles containing core and non-core neurons and terminals
        /// 2. Retrieve existing ensembles
        /// 3. Repair disjointed ensembles
        /// </summary>
        /// <typeparam name="TEnsembleService"></typeparam>
        /// <typeparam name="TParameterSet"></typeparam>
        /// <param name="ensembleService"></param>
        /// <param name="mainEnsemble"></param>
        /// <param name="supplementaryEnsembles"></param>
        /// <param name="parameterSet"></param>
        /// <param name="neuronRepository"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<Neuron> ObtainAsync<TEnsembleService, TParameterSet>(
                this IEnsembleService<TEnsembleService, TParameterSet> ensembleService, 
                Neuron mainEnsemble,
                IEnumerable<Neuron> supplementaryEnsembles,
                TParameterSet parameterSet, 
                INeuronRepository neuronRepository, 
                string userId
            ) 
            where TEnsembleService : IEnsembleService<TEnsembleService, TParameterSet>
            where TParameterSet : IParameterSet
        {   
            Neuron supplementaryParseResult = null;
            if (
                    (
                        // if main ensemble is not specified or 
                        mainEnsemble == null ||
                        // does not contain target, otherwise
                        // ... simply "rotate" the mainEnsemble by assigning it as the result
                        // ... "rotate" - set the reference to another target neuron in an ensemble
                        !ensembleService.TryParse(mainEnsemble, parameterSet, out mainEnsemble)
                    ) &&
                    (
                        // if supplementery ensembles are not specified
                        supplementaryEnsembles == null ||
                        // or target is not in any of them
                        !supplementaryEnsembles.Any(ie => ensembleService.TryParse(ie, parameterSet, out supplementaryParseResult))
                    )
                )
            {
                // retrieve target from DB
                var queries = ensembleService.GetQueries(parameterSet);
                var ensembleFromDB = await neuronRepository.GetByQueriesAsync(userId, queries.ToArray());
                // if target is not in DB
                if (!ensembleService.TryParse(ensembleFromDB, parameterSet, out Neuron dbParseResult))
                {
                    // if main ensemble was not specified
                    if (mainEnsemble == null)
                        // set it to the ensemble from DB
                        mainEnsemble = ensembleFromDB;
                    // otherwise 
                    else
                    {
                        // if supplementeries were specified
                        if (supplementaryEnsembles != null)
                            // add ensemble from DB to supplementaries
                            supplementaryEnsembles = supplementaryEnsembles.Concat(new[] { ensembleFromDB });
                        else
                            // assign ensemble from DB to supplementaries
                            supplementaryEnsembles = new[] { ensembleFromDB };
                    }

                    // build main ensemble
                    await ensembleService.Build(mainEnsemble, supplementaryEnsembles, parameterSet, neuronRepository, userId);
                }
                // else if target is in DB 
                else
                {
                    // if mainEnsemble was not specified
                    if (mainEnsemble == null)
                        // set mainEnsemble
                        mainEnsemble = dbParseResult;
                    else 
                    {
                        // combine db result with supplementaries if latter exist and call build to combine with main ensemble
                        await ensembleService.Build(
                            mainEnsemble,
                            supplementaryEnsembles != null ?
                                supplementaryEnsembles.Concat(new[] { dbParseResult }) :
                                supplementaryEnsembles,
                            parameterSet,
                            neuronRepository,
                            userId
                            );
                    }
                }
            }
            // if target was found in supplementaries
            else if (supplementaryParseResult != null)
            {
                // if mainEnsemble was specified
                if (mainEnsemble != null)
                    // pass supplementaries as it contains target and call build to combine with main ensemble
                    await ensembleService.Build(mainEnsemble, supplementaryEnsembles, parameterSet, neuronRepository, userId);
                else
                    // otherwise set supplementary parse result as main ensemble
                    mainEnsemble = supplementaryParseResult;
            }

            return mainEnsemble;
        }

        internal static bool FindInParamsOrUseTarget<TEnsembleService, TParameterSet>(
                this IEnsembleService<TEnsembleService, TParameterSet> ensembleService,
                Neuron target,
                Neuron mainEnsemble,
                IEnumerable<Neuron> supplementaryEnsembles,
                out Neuron result
            )
            where TEnsembleService : IEnsembleService<TEnsembleService, TParameterSet>
            where TParameterSet : IParameterSet
        {
            result = null;
            Neuron resultInMain = null;

            if (
                // if target is not in mainEnsemble and 
                (result = resultInMain = mainEnsemble.Find(target.Id)) == null &&
                // if supplementaryEnsembles was specified and
                supplementaryEnsembles != null &&
                // target is not in any of the supplementaries
                (result = supplementaryEnsembles.FirstOrDefault(se => se.Find(target.Id) != null)) == null
                )
                result = target;

            return resultInMain != null;
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

        #region Library.Common to Ensemble
        public static Ensembles.Neuron ToEnsemble(this IEnumerable<Library.Common.QueryResult<Library.Common.Neuron>> queryResults)
        {
            var allNs = queryResults.SelectMany(qr => qr.Items.SelectMany(n => n.Traversals.SelectMany(t => t.Neurons)));
            var allTs = queryResults.SelectMany(qr => qr.Items.SelectMany(n => n.Traversals.SelectMany(t => t.Terminals)));

            var eNs = allNs.DistinctBy(n => n.Id)
                .Select(n => n.ToEnsemble()).ToList();
            var eTs = allTs.DistinctBy(t => t.Id)
                .Select(t => t.ToEnsemble(eNs)).ToList();

            return eNs[0];
        }
        #endregion

        #region Neuron
        public static Neuron Find(this Neuron neuron, Guid value, Neuron caller = null)
        {
            Neuron result = null;
            if (neuron.Id == value)
                result = neuron;

            if (result == null)
                result = neuron.Find(value, neuron.Terminals.Select(t => t.Postsynaptic));

            if (result == null)
                result = neuron.Find(value, neuron.Dendrites.Select(t => t.Presynaptic));

            return result;
        }

        private static Neuron Find(this Neuron caller, Guid value, IEnumerable<Neuron> neurons)
        {
            Neuron result = null;
            foreach (var neuron in neurons)
            {
                if (neuron == caller) continue;
                result = neuron.Find(value, caller);
                if (result != null) break;
            }
            return result;
        }

        #endregion
    }
}
