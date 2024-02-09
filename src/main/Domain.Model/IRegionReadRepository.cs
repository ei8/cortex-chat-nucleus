using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public interface IRegionReadRepository
    {
        Task<IEnumerable<Region>> GetAll(CancellationToken token = default);

        Task<IEnumerable<Region>> GetByIds(IEnumerable<Guid> ids, CancellationToken token = default);
    }
}
