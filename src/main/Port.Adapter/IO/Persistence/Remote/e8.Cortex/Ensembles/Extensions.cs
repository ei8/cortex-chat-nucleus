using Nancy.Extensions;
using neurUL.Common.Domain.Model;
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
        /// 1. Create ensembles containing core and non-core neurons and terminals
        /// 2. Retrieve existing ensembles
        /// 3. Repair disjointed ensembles
        /// </summary>
        /// <typeparam name="TEnsembleService"></typeparam>
        /// <typeparam name="TParameterSet"></typeparam>
        /// <param name="ensembleService"></param>
        /// <param name="ensembles"></param>
        /// <param name="parameterSet"></param>
        /// <param name="neuronRepository"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<Neuron> ObtainAsync<TEnsembleService, TParameterSet>(
                this IEnsembleService<TEnsembleService, TParameterSet> ensembleService, 
                EnsembleCollection ensembles,
                TParameterSet parameterSet, 
                INeuronRepository neuronRepository, 
                string userId
            ) 
            where TEnsembleService : IEnsembleService<TEnsembleService, TParameterSet>
            where TParameterSet : IParameterSet
        {
            AssertionConcern.AssertArgumentNotNull(ensembles, nameof(ensembles));
            Neuron result = null;
            Neuron ensembleParseResult = null;
            // if target is not in any of specified ensembles
            if (!ensembles.Items.Any(ie => ensembleService.TryParse(ie, parameterSet, out ensembleParseResult)))
            {
                // retrieve target from DB
                var queries = ensembleService.GetQueries(parameterSet);
                var ensembleFromDB = await neuronRepository.GetByQueriesAsync(userId, queries.ToArray());
                // if target is not in DB
                if (!ensembleService.TryParse(ensembleFromDB, parameterSet, out Neuron dbParseResult))
                {
                    // add to ensembles
                    ensembles.PreciseAdd(ensembleFromDB);
                    // build in ensembles
                    await ensembleService.Build(ensembles, parameterSet, neuronRepository, userId);
                }
                // else if target is in DB 
                else
                {
                    ensembles.PreciseAdd(dbParseResult);
                    result = dbParseResult;
                }
            }
            // if target was found in ensembles
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
        public static IEnumerable<Neuron> GetAllNeurons(this Neuron neuron)
        {
            List<Neuron> result = new List<Neuron>();

            neuron.GetAllNeuronsCore(result);

            return result.ToArray();
        }

        private static void GetAllNeuronsCore(this Neuron neuron, IList<Neuron> allNeurons)
        {
            // if neuron with same id does not yet exist
            if (!allNeurons.Any(n => n.Id == neuron.Id))
                allNeurons.Add(neuron);
            // otherwise 
            else
                // assert neuron with same id exists and is the same instance
                AssertionConcern.AssertStateTrue(allNeurons.Contains(neuron), "Ensemble contains at least two Neurons with the same Id.");

            // sift through postsynaptics
            neuron.GetAllNeuronsCore(allNeurons, neuron.Terminals.Select(t => t.Postsynaptic));

            // sift through presynaptics
            neuron.GetAllNeuronsCore(allNeurons, neuron.Dendrites.Select(t => t.Presynaptic));
        }

        private static void GetAllNeuronsCore(this Neuron currentNeuron, IList<Neuron> allNeurons, IEnumerable<Neuron> neurons)
        {
            // for each neuron in list
            foreach (var neuron in neurons)
            {
                // if neuron is already in the list, skip it
                if (allNeurons.Contains(neuron)) continue;
                // otherwise find it in current neuron in list, using the list's parent as the caller
                neuron.GetAllNeuronsCore(allNeurons);
            }
        }
        #endregion
    }
}
