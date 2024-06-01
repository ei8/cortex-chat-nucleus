using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class Neuron
    {
        private Dictionary<Guid, Terminal> terminals = new Dictionary<Guid, Terminal>();
        private Dictionary<Guid, Terminal> dendrites = new Dictionary<Guid, Terminal>();

        public Neuron(Guid id, string tag, string externalReferenceUrl, Guid? regionId) : this(id, false, tag, externalReferenceUrl, regionId)
        {
        }

        public Neuron(Guid id, bool isTransient, string tag, string externalReferenceUrl, Guid? regionId)
        {
            this.Id = id;
            this.IsTransient = isTransient;
            this.Tag = tag;
            this.ExternalReferenceUrl = externalReferenceUrl;
            this.RegionId = regionId;
        }

        internal void AddDendrite(Terminal value)
        {
            dendrites.Add(value.Id, value);
        }

        internal void AddTerminal(Terminal value)
        {
            terminals.Add(value.Id, value);
        }

        internal Neuron CloneWithoutTerminals() =>
            new Neuron(
                this.Id,
                this.IsTransient,
                this.Tag,
                this.ExternalReferenceUrl,
                this.RegionId
            );

        public static Neuron CreateTransient() => Neuron.CreateTransient(null, null, null);

        public static Neuron CreateTransient(string tag, string externalReferenceUrl, Guid? regionId) => new Neuron(Guid.NewGuid(), true, tag, externalReferenceUrl, regionId);

        internal void RemoveDendrite(Guid value)
        {
            dendrites.Remove(value);
        }

        internal void RemoveTerminal(Guid value)
        {
            terminals.Remove(value);
        }

        public IEnumerable<Terminal> Dendrites => dendrites.Values;
        public string ExternalReferenceUrl { get; private set; }
        public Guid Id { get; private set; }
        public bool IsTransient { get; private set; }
        public Guid? RegionId { get; private set; }
        public string Tag { get; private set; }
        public IEnumerable<Terminal> Terminals => terminals.Values;
    }
}
