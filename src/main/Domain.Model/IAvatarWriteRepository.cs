using ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    /// <summary>
    /// Provides functionality for writing Avatars.
    /// </summary>
    public interface IAvatarWriteRepository
    {
        /// <summary>
        /// Saves the specified Avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task Save(Avatar avatar, CancellationToken token = default);
    }
}
