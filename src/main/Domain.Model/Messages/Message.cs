using ei8.Cortex.Coding.Properties;
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

        [neurULClass]
        public Guid ContentId { get; set; }
        
        [neurULClass(typeof(Avatar))]
        public Guid SenderId { get; set; }

        [neurULNeuronProperty]
        public string MirrorUrl { get; set; }

        [neurULNeuronProperty]
        public Guid? RegionId { get; set; }
    }
}
