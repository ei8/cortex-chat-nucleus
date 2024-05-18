using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
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
            this.terminals.Add(value.Id, value);
        }

        internal void RemoveTerminal(Guid value)
        {
            this.terminals.Remove(value);
        }

        internal void AddDendrite(Terminal value)
        {
            this.dendrites.Add(value.Id, value);
        }
        
        internal void RemoveDendrite(Guid value)
        {
            this.dendrites.Remove(value);
        }

        public IEnumerable<Terminal> Terminals => this.terminals.Values;

        public IEnumerable<Terminal> Dendrites => this.dendrites.Values;

    }
}
