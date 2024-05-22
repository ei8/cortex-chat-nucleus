using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public interface IEnsemble
    {
        IEnumerable<LevelParser> LevelEvaluators { get; }

        bool TryGet(Neuron value, out Neuron result);
    }
}
