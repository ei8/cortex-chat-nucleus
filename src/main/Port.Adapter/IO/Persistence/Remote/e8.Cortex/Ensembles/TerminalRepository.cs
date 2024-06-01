using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class TerminalRepository : ITerminalRepository
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;

        public TerminalRepository(INeuronQueryClient neuronQueryClient, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(neuronQueryClient));

            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
        }

        public async Task<IEnumerable<Terminal>> GetByIdsAsync(Guid presynapticId, string userId, params Guid[] postsynapticIds)
        {
            var qr = await neuronQueryClient.GetNeuronsInternal(
                settingsService.CortexLibraryOutBaseUrl + "/",
                presynapticId.ToString(),
                new NeuronQuery()
                {
                    Id = postsynapticIds.Select(p => p.ToString()),
                    RelativeValues = RelativeValues.Postsynaptic,
                    PageSize = postsynapticIds.Length
                },
                userId
                );

            var result = new List<Terminal>();

            foreach (var postsynapticId in postsynapticIds)
            {
                var retrievedNeurons = qr.Items.Where(n =>
                    n.Terminal.PresynapticNeuronId == presynapticId.ToString() &&
                    n.Terminal.PostsynapticNeuronId == postsynapticId.ToString()
                    );
                AssertionConcern.AssertStateTrue(retrievedNeurons.Count() < 2, "Multiple identical Terminals found.");

                var t = retrievedNeurons.Single().Terminal;
                var resultTerminal = new Terminal(
                    Guid.Parse(t.Id),
                    false,
                    float.Parse(t.Strength),
                    Enum.TryParse(t.Effect, out NeurotransmitterEffect ne) ? ne : NeurotransmitterEffect.Excite
                    );

                result.Add(resultTerminal);
            }

            return result;
        }
    }
}
