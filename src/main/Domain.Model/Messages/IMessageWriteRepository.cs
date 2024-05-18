using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    public interface IMessageWriteRepository
    {
        Task Save(Message message, string userId, CancellationToken token = default);
    }
}
