using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    public interface IRecipientWriteRepository
    {
        Task SaveAll(IEnumerable<Recipient> recipients, string userId, CancellationToken token = default);
    }
}
