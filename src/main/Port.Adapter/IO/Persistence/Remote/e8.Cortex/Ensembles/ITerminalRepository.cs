using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public interface ITerminalRepository
    {
        Task<IEnumerable<Terminal>> GetByIdsAsync(Guid presynapticId, string userId, params Guid[] postsynapticIds);
    }
}
