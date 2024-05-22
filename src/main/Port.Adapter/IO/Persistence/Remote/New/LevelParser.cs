using System;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public class LevelParser
    {
        public LevelParser(params IFilter[] filters)
        {
            this.Filters = filters;
        }

        public IEnumerable<IFilter> Filters { get; private set; }

        public IEnumerable<Neuron> Evaluate(IEnumerable<Neuron> paths)
        {
            var result = new List<Neuron>();
            foreach(var filter in this.Filters)
            {
                result.AddRange(filter.Evaluate(paths));
            }
            return result;
        }
    }
}
