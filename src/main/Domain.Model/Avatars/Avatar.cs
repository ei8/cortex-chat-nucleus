using ei8.Cortex.Coding.Model;
using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Avatars
{
    /// <summary>
    /// Represents an Avatar.
    /// </summary>
    public class Avatar : CreatedInstanceBase
    {
        public Avatar()
        {
        }

        public Avatar(Guid id, string name, DateTimeOffset? creationTimestamp)
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(id)
            );
            AssertionConcern.AssertArgumentNotEmpty(
                name,
                "Avatar name cannot be null or empty.",
                nameof(name)
            );
            AssertionConcern.AssertArgumentNotNull(
                creationTimestamp.GetValueOrDefault(),
                nameof(creationTimestamp)
            );

            this.Id = id;
            this.Name = name;
            this.CreationTimestamp = creationTimestamp;
        }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name { get; set; }
    }
}
