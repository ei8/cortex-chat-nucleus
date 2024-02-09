using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    public interface IDestinationWriteRepository
    {
        Task SaveAll(IEnumerable<Destination> destination, CancellationToken token = default);
    }
}
