using ei8.Cortex.Coding.Properties;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars
{
    /// <summary>
    /// Represents an Avatar.
    /// </summary>
    public class Avatar
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        [neurULNeuronProperty]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name { get; set; }
    }
}
