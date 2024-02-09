using ei8.Cortex.Chat.Nucleus.Domain.Model;
using neurUL.Common.Domain.Model;
using System;
using ei8.Cortex.Library.Client.Out;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Library.Common;
using System.Linq;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Process.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;
        private readonly IIdentityService identityService;

        public LibraryService(INeuronQueryClient neuronQueryClient, ISettingsService settingsService, IIdentityService identityService)
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(identityService, nameof(identityService));

            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
            this.identityService = identityService;
        }

        public async Task<Guid> GetInstantiatesMessageId()
        {
            var instantiatesMessage = (
                await this.neuronQueryClient.GetNeuronsInternal(
                    this.settingsService.CortexLibraryOutBaseUrl + "/",
                    new NeuronQuery()
                    {
                        ExternalReferenceUrl = new string[] { this.settingsService.InstantiatesMessageExternalReferenceUrl },
                        SortBy = SortByValue.NeuronCreationTimestamp,
                        SortOrder = SortOrderValue.Descending
                    },
                    this.identityService.UserId
                )
            ).Items.SingleOrDefault();

            AssertionConcern.AssertStateTrue(Guid.TryParse(instantiatesMessage?.Id, out Guid result), "'Instantiates, Message' neuron was not found.");

            return result;
        }
    }
}
