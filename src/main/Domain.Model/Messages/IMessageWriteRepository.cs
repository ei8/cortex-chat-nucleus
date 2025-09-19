using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Provides functionality for writing Mesages.
    /// </summary>
    public interface IMessageWriteRepository
    {
        /// <summary>
        /// Saves the specified Message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task Save(Message message, CancellationToken token = default);
    }
}
