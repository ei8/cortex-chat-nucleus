using ei8.Cortex.Coding;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Property names based on function in Domain.Model.Library.TagValues
    /// </summary>
    public class Message
    {
        [neurULNeuronProperty]
        public Guid Id { get; set; }

        public string Content { get; set; }

        [neurULClass(typeof(Avatar))]
        public Guid SenderId { get; set; }

        public DateTimeOffset? CreationTimestamp { get; set; }

        public DateTimeOffset? LastModificationTimestamp { get; set; }

        [neurULNeuronProperty]
        public Guid? RegionId { get; set; }

        [neurULNeuronProperty]
        public string ExternalReferenceUrl { get; set; }
        
        [neurULNeuronProperty(nameof(Neuron.CreationTimestamp), true)]
        public DateTimeOffset? NeuronCreationTimestamp { get; set; }
        
        [neurULNeuronProperty(true)]
        public Guid CreationAuthorId { get; set; }
        
        [neurULNeuronProperty(true)]
        public string CreationAuthorTag { get; set; }
        
        [neurULNeuronProperty(true)]
        public DateTimeOffset? UnifiedLastModificationTimestamp { get; set; }
        
        [neurULNeuronProperty(true)]
        public Guid? UnifiedLastModificationAuthorId { get; set; }
        
        [neurULNeuronProperty(true)]
        public string UnifiedLastModificationAuthorTag { get; set; }
    }
}
