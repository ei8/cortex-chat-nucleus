using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class EnsembleCollection
    {
        private class NeuronIndexPair
        {
            public Neuron Neuron;
            public int Index;
        }
        private readonly List<Neuron> neuronList;
        
        public EnsembleCollection()
        {
            this.neuronList = new List<Neuron>();
        }

        public bool TryFind(
                Neuron target,
                out Neuron result,
                out int resultEnsembleIndex
            )
        {
            result = null;
            resultEnsembleIndex = -1;

            foreach (var n in this.neuronList)
            {
                var foundNeuron = n.GetAllNeurons().SingleOrDefault(an => an.Id == target.Id);
                if (foundNeuron != null)
                {
                    result = foundNeuron;
                    resultEnsembleIndex = this.neuronList.IndexOf(n);
                    break;
                }
            }

            return resultEnsembleIndex != -1;
        }

        public void PreciseAdd(Neuron item)
        {
            var allInList = this.neuronList.SelectMany(n => n.GetAllNeurons());
            var allInItem = item.GetAllNeurons();
            // are there any neurons in specified neuron that can be found in any of the contained ensembles
            var commonInItem = allInItem.Where(i => allInList.Any(n => n.Id == i.Id));
            if (commonInItem.Count() > 0)
            {
                var ensembleIndices = commonInItem
                    .Select(ci => this.TryFind(ci, out Neuron fn, out int rei) ? rei : rei)
                    .Where(i => i > -1);
                // if all common items are in the same ensemble
                if (ensembleIndices.Distinct().Count() == 1)
                {

                }
                // otherwise
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
                this.neuronList.Add(item);
        }

        public IEnumerable<Neuron> Items => this.neuronList.ToArray();
    }
}
