using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Library
{
    public interface ILibraryService
    {
        Task<Guid> GetId(string tag);
    }
}
