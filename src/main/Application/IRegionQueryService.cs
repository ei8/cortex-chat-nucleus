using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public interface IRegionQueryService
    {
        Task<IEnumerable<Common.RegionResult>> GetRegions(CancellationToken token = default);

        Task<IEnumerable<Common.RegionResult>> GetRegions(IEnumerable<Guid> ids, CancellationToken token = default);
    }
}
