using ei8.Cortex.Library.Client.Out;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class TerminalService : ITerminalService
    {
        private readonly ITerminalRepository terminalRepository;

        public TerminalService(ITerminalRepository terminalRepository)
        {
            this.terminalRepository = terminalRepository;
        }

        public async Task<IEnumerable<Terminal>> ObtainLinkAsync(Neuron presynaptic, string userId, params Neuron[] postsynaptics)
        {
            var ts = await this.terminalRepository.GetByIdsAsync(presynaptic.Id, userId, postsynaptics.Select(p => p.Id).ToArray());

            var results = new List<Terminal>(ts);
            foreach (Neuron postsynaptic in postsynaptics)
            {
                var result = results.SingleOrDefault(r => r.Presynaptic.Id == presynaptic.Id && r.Postsynaptic.Id == postsynaptic.Id);
                if (result == null)
                    result = new Terminal();

                result.Link(presynaptic, postsynaptic);

                results.Add(result);
            }

            return results;
        }
    }
}
