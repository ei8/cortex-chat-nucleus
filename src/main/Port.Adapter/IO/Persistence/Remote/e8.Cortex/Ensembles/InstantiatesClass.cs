using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Data;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters;
using ei8.Cortex.Library.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class InstantiatesClass : IEnsemble<InstantiatesClass, InstantiatesClass.IdProvider>
    {
        public InstantiatesClass()
        {
        }

        public Neuron Neuron { get; private set; }

        public IEnumerable<Library.Common.NeuronQuery> GetDataQueries(InstantiatesClass.IdProvider ensembleIdProvider) =>
            new[] {
                new NeuronQuery()
                {
                    Id = new[] { ensembleIdProvider.Class.ToString() },
                    DirectionValues = DirectionValues.Any,
                    Depth = 3,
                    TraversalMinimumDepthPostsynaptic = new[] {
                        new DepthIdsPair {
                            Depth = 3,
                            Ids = new[] {
                                ensembleIdProvider.Subordination,
                                ensembleIdProvider.InstantiatesUnit
                            }
                        }
                    }
                }
            };

        public bool TryParse(EnsembleData value, InstantiatesClass.IdProvider neuronIdProvider, out InstantiatesClass result)
        {
            result = null;
            IEnumerable<Neuron> paths = new[] { value.ToEnsemble(neuronIdProvider.Class) };

            var levelParsers = new LevelParser[]
            {
                new LevelParser(new PresynapticBySibling(neuronIdProvider.DirectObject)),
                new LevelParser(new PresynapticBySibling(
                    neuronIdProvider.Subordination,
                    neuronIdProvider.InstantiatesUnit
                    ))
            };

            foreach (var levelParser in levelParsers)
                paths = levelParser.Evaluate(paths);

            if (paths.Count() == 1)
                result = new InstantiatesClass() { Neuron = paths.ToArray()[0] };

            return result != null;
        }

        public class IdProvider : IEnsembleIdProvider
        {
            private readonly Guid @class;
            private readonly Guid directObject;
            private readonly Guid subordination;
            private readonly Guid instantiatesUnit;

            public IdProvider(
                Guid @class,
                Guid directObject,
                Guid subordination,
                Guid instantiatesUnit
                )
            {
                this.@class = @class;
                this.directObject = directObject;
                this.subordination = subordination;
                this.instantiatesUnit = instantiatesUnit;
            }

            public Guid Class => @class;

            public Guid DirectObject => directObject;

            public Guid Subordination => subordination;

            public Guid InstantiatesUnit => instantiatesUnit;

        }
    }
}
