using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.EnsembleServices
{
    public class Instantiates : IInstantiates
    {
        private readonly ICoreSet coreSet;
        private readonly IDependency dependency;

        public Instantiates(ICoreSet coreSet, IDependency dependency)
        {
            this.coreSet = coreSet;
            this.dependency = dependency;
        }

        public async Task<Neuron> BuildAsync(Ensemble ensemble, IInstantiatesParameterSet parameterSet, INeuronRepository neuronRepository, string userId)
        {
            var subordination = ensemble.Obtain(this.coreSet.Subordination);
            var instantiatesUnit = ensemble.Obtain(this.coreSet.InstantiatesUnit);
            var classDirectObject = await this.dependency.ObtainAsync(
                ensemble, 
                new DependencyParameterSet(
                    parameterSet.Class,
                    this.coreSet.DirectObject
                ),
                neuronRepository,
                userId
                );
            var instantiatesClass = ensemble.Obtain(Neuron.CreateTransient(null, null, null));
            ensemble.Obtain(Terminal.CreateTransient(instantiatesClass.Id, subordination.Id));
            ensemble.Obtain(Terminal.CreateTransient(instantiatesClass.Id, instantiatesUnit.Id));
            ensemble.Obtain(Terminal.CreateTransient(instantiatesClass.Id, classDirectObject.Id));

            return instantiatesClass;
        }

        public IEnumerable<Library.Common.NeuronQuery> GetQueries(IInstantiatesParameterSet parameterSet) =>
            new[] {
                new NeuronQuery()
                {
                    Id = new[] { parameterSet.Class.Id.ToString() },
                    DirectionValues = DirectionValues.Any,
                    Depth = 3,
                    TraversalMinimumDepthPostsynaptic = new[] {
                        new DepthIdsPair {
                            Depth = 3,
                            Ids = new[] {
                                this.coreSet.Subordination.Id,
                                this.coreSet.InstantiatesUnit.Id
                            }
                        }
                    }
                }
            };

        public bool TryParse(Ensemble ensemble, IInstantiatesParameterSet parameterSet, out Neuron result)
        {
            result = null;
            IEnumerable<Neuron> neurons = new[] { parameterSet.Class };

            var levelParsers = new LevelParser[]
            {
                new LevelParser(new PresynapticBySibling(this.coreSet.DirectObject.Id)),
                new LevelParser(new PresynapticBySibling(
                    this.coreSet.Subordination.Id,
                    this.coreSet.InstantiatesUnit.Id
                    ))
            };

            foreach (var levelParser in levelParsers)
                neurons = levelParser.Evaluate(ensemble, neurons);

            if (neurons.Count() == 1)
                result = neurons.Single();

            return result != null;
        }
    }
}
