using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.d23.neurULization.Processors.Writers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class neurULizer : IneurULizer
    {
        private readonly IEnsembleRepository ensembleRepository;
        private readonly IInstanceProcessor writersInstanceProcessor;
        private readonly Coding.d23.neurULization.Processors.Readers.Inductive.IInstanceProcessor readersInductiveInstanceProcessor;
        private readonly IPrimitiveSet primitives;
        private readonly IDictionary<string, Ensemble> ensembleCache;
        private readonly IGrannyService grannyService;
        private readonly ISettingsService settingsService;

        public neurULizer(
            IEnsembleRepository ensembleRepository, 
            Coding.d23.neurULization.Processors.Writers.IInstanceProcessor writersInstanceProcessor,
            Coding.d23.neurULization.Processors.Readers.Inductive.IInstanceProcessor readersInductiveInstanceProcessor,
            IPrimitiveSet primitives, 
            IDictionary<string, Ensemble> ensembleCache,
            IGrannyService grannyService,
            ISettingsService settingsService
        )
        {
            this.ensembleRepository = ensembleRepository;
            this.writersInstanceProcessor = writersInstanceProcessor;
            this.readersInductiveInstanceProcessor = readersInductiveInstanceProcessor;
            this.primitives = primitives;
            this.ensembleCache = ensembleCache;
            this.grannyService = grannyService;
            this.settingsService = settingsService;
        }

        public async Task<Ensemble> neurULizeAsync<TValue>(TValue value, string userId)
            => await this.neurULizeAsync(
                value,
                this.ensembleRepository, 
                this.writersInstanceProcessor,
                this.ensembleCache,
                this.settingsService.AppUserId,
                this.settingsService.CortexLibraryOutBaseUrl + "/",
                this.settingsService.QueryResultLimit,
                userId
            );

        public async Task<IEnumerable<TValue>> DeneurULizeAsync<TValue>(Ensemble value, string userId)
            where TValue : class, new()
            => await this.DeneurULizeAsync<TValue>(
                value,
                this.ensembleRepository, 
                this.readersInductiveInstanceProcessor,
                this.primitives,
                this.grannyService,
                this.settingsService.AppUserId,
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                this.settingsService.CortexLibraryOutBaseUrl + "/",
                this.settingsService.QueryResultLimit,
                userId
            );
    }
}
