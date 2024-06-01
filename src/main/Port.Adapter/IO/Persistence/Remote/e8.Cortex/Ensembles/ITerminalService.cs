using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public interface ITerminalService
    {
        Task<IEnumerable<Terminal>> ObtainLinkAsync(Neuron presynaptic, string userId, params Neuron[] postsynaptics);
    }
}
