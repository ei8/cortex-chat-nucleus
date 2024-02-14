using ei8.Cortex.Chat.Common;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public class RegionQueryService : IRegionQueryService
    {
        private readonly IRegionReadRepository regionRepository;

        public RegionQueryService(IRegionReadRepository regionRepository)
        {
            AssertionConcern.AssertArgumentNotNull(regionRepository, nameof(regionRepository));

            this.regionRepository = regionRepository;
        }

        public async Task<IEnumerable<RegionResult>> GetRegions(CancellationToken token = default) => 
            (await this.regionRepository.GetAll(token)).Select(r => r.ToCommon());

        public async Task<IEnumerable<RegionResult>> GetRegions(IEnumerable<Guid> ids, CancellationToken token = default)
        {
            return (await this.regionRepository.GetByIds(ids, token)).Select(r => r.ToCommon());
        }
    }
}
