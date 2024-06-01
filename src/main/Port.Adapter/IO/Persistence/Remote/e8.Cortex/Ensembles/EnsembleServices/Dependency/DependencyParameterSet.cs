using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.EnsembleServices
{
    public class DependencyParameterSet : IDependencyParameterSet
    {
        public DependencyParameterSet(
            Neuron value,
            Neuron type
            )
        {
            AssertionConcern.AssertArgumentNotNull(value, nameof(value));
            AssertionConcern.AssertArgumentNotNull(type, nameof(type));

            this.Value = value;
            this.Type = type;
        }

        public Neuron Value { get; }

        public Neuron Type { get; }
    }
}
