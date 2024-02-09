using System;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public interface ILibraryService
    {
        Task<Guid> GetInstantiatesMessageId();
    }
}
