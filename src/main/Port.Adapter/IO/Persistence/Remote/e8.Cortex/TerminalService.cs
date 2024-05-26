using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex
{
    public class TerminalService : ITerminalService
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly ISettingsService settingsService;

        public TerminalService(INeuronQueryClient neuronQueryClient, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(neuronQueryClient));

            this.neuronQueryClient = neuronQueryClient;
            this.settingsService = settingsService;
        }
        
        public async Task<IEnumerable<Ensembles.Terminal>> GetOrCreateTerminalsIfNotExistsAsync(Ensembles.Neuron presynaptic, string userId, params Ensembles.Neuron[] postsynaptics)
        {
            var qr = await this.neuronQueryClient.GetNeuronsInternal(
                this.settingsService.CortexLibraryOutBaseUrl + "/",
                presynaptic.Id.ToString(),
                new NeuronQuery()
                {
                    Id = postsynaptics.Select(p => p.Id.ToString()),
                    RelativeValues = RelativeValues.Postsynaptic, 
                    PageSize = postsynaptics.Length
                },
                userId
                );

            var result = new List<Ensembles.Terminal>();

            foreach (var postsynaptic in postsynaptics)
            {
                var retrievedNeurons = qr.Items.Where(n => 
                    n.Terminal.PresynapticNeuronId == presynaptic.Id.ToString() && 
                    n.Terminal.PostsynapticNeuronId == postsynaptic.Id.ToString()
                    );
                AssertionConcern.AssertStateTrue(retrievedNeurons.Count() < 2, "Multiple identical Terminals found.");

                var resultTerminal = new Ensembles.Terminal();
                if (retrievedNeurons.Count() == 1)
                {
                    var t = retrievedNeurons.Single().Terminal;
                    resultTerminal.Id = Guid.Parse(t.Id);
                    resultTerminal.IsTransient = false;
                    resultTerminal.Strength = float.Parse(t.Strength);
                    resultTerminal.Effect = Enum.TryParse(t.Effect, out NeurotransmitterEffect ne) ? ne : NeurotransmitterEffect.Excite;
                }
                else
                {
                    resultTerminal.Id = Guid.NewGuid();
                    resultTerminal.IsTransient = true;
                    resultTerminal.Strength = 1f;
                    resultTerminal.Effect = NeurotransmitterEffect.Excite;
                }

                resultTerminal.Presynaptic = presynaptic;
                resultTerminal.Postsynaptic = postsynaptic;
                presynaptic.AddTerminal(resultTerminal);
                postsynaptic.AddDendrite(resultTerminal);

                result.Add(resultTerminal);
            }

            return result;
        }

        public Ensembles.Terminal Unlink(Ensembles.Neuron presynaptic, Ensembles.Neuron postsynaptic)
        {
            var result = presynaptic.Terminals.SingleOrDefault(
                t => t.Presynaptic.Id == presynaptic.Id && t.Postsynaptic.Id == postsynaptic.Id
            );
            result.Presynaptic = null;
            result.Postsynaptic = null;
            presynaptic.RemoveTerminal(result.Id);
            postsynaptic.RemoveDendrite(result.Id);

            return result;
        }
    }
}
