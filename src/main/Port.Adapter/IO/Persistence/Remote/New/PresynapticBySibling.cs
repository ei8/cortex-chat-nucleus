using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
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

        public IEnumerable<Neuron> Evaluate(IEnumerable<Neuron> paths)
        {
            var result = new List<Neuron>();

            // loop through each specified neuron
            foreach (var n in paths)
            {
                // loop through each presynaptic
                foreach (var pre in n.Dendrites.Select(d => d.Presynaptic))
                {
                    // if exhaustive
                    if (this.exhaustive)
                    {
                        // if presynaptic has only current neuron + siblings as postsynaptic 
                        if (pre.Terminals.Count() == this.siblingNeuronIds.Count() + 1 &&
                            // and postsynaptics match the current neuron and the siblings
                            pre.Terminals.Select(t => t.Postsynaptic.Id).HasSameElementsAs(
                                this.siblingNeuronIds.Concat(new[] { n.Id })) 
                            )
                        {
                            result.Add(pre);
                        }
                    }
                }
            }

            return result;
        }
    }
}
