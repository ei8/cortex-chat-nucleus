using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public interface IAvatarWriteRepository
    {
        Task Save(Avatar avatar, CancellationToken token = default);
    }
}
