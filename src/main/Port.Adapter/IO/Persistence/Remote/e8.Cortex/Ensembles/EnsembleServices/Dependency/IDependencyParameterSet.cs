﻿using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.EnsembleServices
{
    public interface IDependencyParameterSet : IParameterSet
    {
        Neuron Value { get; }

        Neuron Type { get; }
    }
}
