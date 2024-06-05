using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters
{
    public class PresynapticBySibling : IFilter
    {
        private readonly bool exhaustive;
        private readonly IEnumerable<Guid> siblingNeuronIds;

        public PresynapticBySibling(params Guid[] siblingNeuronIds) : this(true, siblingNeuronIds)
        {
        }

        public PresynapticBySibling(bool exhaustive, params Guid[] siblingNeuronIds)
        {
            this.exhaustive = exhaustive;
            this.siblingNeuronIds = siblingNeuronIds;
        }

        public IEnumerable<Neuron> Evaluate(Ensemble ensemble, IEnumerable<Neuron> neurons)
        {
            var result = new List<Neuron>();

            // loop through each specified neuron
            foreach (var neuron in neurons)
            {
                // loop through each presynaptic
                foreach (var pre in neuron.GetPresynapticNeurons(ensemble))
                {
                    // if exhaustive
                    if (exhaustive)
                    {
                        var preTerminals = pre.GetTerminals(ensemble);
                        // if presynaptic has only current neuron + siblings as postsynaptic 
                        if (preTerminals.Count() == siblingNeuronIds.Count() + 1 &&
                            // and postsynaptics of presynaptic match the current neuron and the siblings
                            preTerminals.Select(t => t.PostsynapticNeuronId).HasSameElementsAs(
                                siblingNeuronIds.Concat(new[] { neuron.Id }))
                            )
                        {
                            // return presynaptic
                            result.Add(pre);
                        }
                    }
                }
            }

            return result;
        }
    }
}
