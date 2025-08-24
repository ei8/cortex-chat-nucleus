using ei8.Cortex.Coding.Properties;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public class Avatar
    {
        public string Name { get; set; }

        public DateTimeOffset? CreationTimestamp { get; set; }

        public DateTimeOffset? LastModificationTimestamp { get; set; }

        #region Neuron Properties
        [neurULNeuronProperty]
        public Guid Id { get; set; }

        [neurULNeuronProperty]
        public string MirrorUrl { get; set; }

        [neurULNeuronProperty]
        public string Url { get; set; }
        #endregion
    }
}
