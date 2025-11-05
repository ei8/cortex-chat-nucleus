using ei8.Cortex.Coding.Model;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars
{
    /// <summary>
    /// Represents an Avatar.
    /// </summary>
    public class Avatar : CreatedInstanceBase
    {
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name { get; set; }
    }
}
