using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public interface ITerminalService
    {
        Task<IEnumerable<Terminal>> GetOrCreateTerminalsIfNotExistsAsync(Neuron presynaptic, string userId, params Neuron[] postsynaptic);

        Terminal Unlink(Neuron presynaptic, Neuron postsynaptic);
    }
}
