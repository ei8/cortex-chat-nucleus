using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.EnsembleServices
{
    public class Dependency : IDependency
    {
        private readonly ICoreSet coreSet;

        public Dependency(ICoreSet coreSet)
        {
            this.coreSet = coreSet;
        }

        
        public async Task<Neuron> BuildAsync(Ensemble ensemble, IDependencyParameterSet parameterSet, INeuronRepository neuronRepository, string userId)
        {
            Neuron value = ensemble.Obtain(parameterSet.Value);
            Neuron dependencyType = ensemble.Obtain(parameterSet.Type);
            Neuron dependency = ensemble.Obtain(Neuron.CreateTransient(null, null, null));
            // add dependency to ensemble
            ensemble.Obtain(Terminal.CreateTransient(dependency.Id, value.Id));
            ensemble.Obtain(Terminal.CreateTransient(dependency.Id, dependencyType.Id));
            return dependency;
        }

        public IEnumerable<Library.Common.NeuronQuery> GetQueries(IDependencyParameterSet parameterSet) =>
            new[] {
                new NeuronQuery()
                {
                    Postsynaptic = new[] { 
                        parameterSet.Value.Id.ToString(),
                        parameterSet.Type.ToString()
                    },
                    DirectionValues = DirectionValues.Outbound,
                    Depth = 1
                }
            };

        public bool TryParse(Ensemble ensemble, IDependencyParameterSet parameterSet, out Neuron result)
        {
            result = null;
            IEnumerable<Neuron> neurons = new[] { parameterSet.Value };

            var levelParsers = new LevelParser[]
            {
                new LevelParser(new PresynapticBySibling(parameterSet.Type.Id)),
            };

            foreach (var levelParser in levelParsers)
                neurons = levelParser.Evaluate(ensemble, neurons);

            if (neurons.Count() == 1)
                result = neurons.Single();

            return result != null;

        }
    }
}
