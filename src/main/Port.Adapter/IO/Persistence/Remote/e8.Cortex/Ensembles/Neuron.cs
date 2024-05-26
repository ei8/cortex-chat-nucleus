using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class Neuron
    {
        private Dictionary<Guid, Terminal> terminals = new Dictionary<Guid, Terminal>();
        private Dictionary<Guid, Terminal> dendrites = new Dictionary<Guid, Terminal>();

        internal Neuron() { }

        public Guid Id { get; internal set; }
        public bool IsTransient { get; internal set; }
        public string Tag { get; internal set; }
        public Guid? RegionId { get; internal set; }
        public string ExternalReferenceUrl { get; internal set; }

        internal void AddTerminal(Terminal value)
        {
            terminals.Add(value.Id, value);
        }

        internal void RemoveTerminal(Guid value)
        {
            terminals.Remove(value);
        }

        internal void AddDendrite(Terminal value)
        {
            dendrites.Add(value.Id, value);
        }

        internal void RemoveDendrite(Guid value)
        {
            dendrites.Remove(value);
        }

        public IEnumerable<Terminal> Terminals => terminals.Values;

        public IEnumerable<Terminal> Dendrites => dendrites.Values;

    }
}
