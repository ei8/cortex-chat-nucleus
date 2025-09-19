using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Provides functionality for writing Communicators.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommunicatorWriteRepository<T> where T : CommunicatorBase, new()
    {
        /// <summary>
        /// Saves all specified CommunicatorBase values.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SaveAll(IEnumerable<T> values, CancellationToken token = default);
    }
}
