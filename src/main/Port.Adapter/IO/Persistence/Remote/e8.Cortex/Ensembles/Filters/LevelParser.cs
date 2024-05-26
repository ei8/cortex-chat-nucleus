using System;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters
{
    public class LevelParser
    {
        public LevelParser(params IFilter[] filters)
        {
            Filters = filters;
        }

        public IEnumerable<IFilter> Filters { get; private set; }

        public IEnumerable<Neuron> Evaluate(IEnumerable<Neuron> paths)
        {
            var result = new List<Neuron>();
            foreach (var filter in Filters)
            {
                result.AddRange(filter.Evaluate(paths));
            }
            return result;
        }
    }
}
