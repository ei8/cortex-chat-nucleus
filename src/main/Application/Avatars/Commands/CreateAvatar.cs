using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Chat.Nucleus.Application.Avatars.Commands
{
    /// <summary>
    /// Represents a CreateAvatar command.
    /// </summary>
    public class CreateAvatar : ICommand
    {
        /// <summary>
        /// Contructs a CreateAvatar command.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="regionId"></param>
        /// <param name="mirrorUrl"></param>
        /// <param name="userId"></param>
        public CreateAvatar(
            Guid id, 
            string name, 
            Guid? regionId, 
            string mirrorUrl, 
            string userId
        )
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(id)
            );
            AssertionConcern.AssertArgumentNotNull(name, nameof(name));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                regionId,
                Domain.Model.Constants.Exception.InvalidId,
                nameof(regionId)
                );
            AssertionConcern.AssertArgumentNotEmpty(userId, Constants.Exception.InvalidUserId, nameof(userId));

            this.Id = id;
            this.Name = name;
            this.RegionId = regionId;
            this.MirrorUrl = mirrorUrl;
            this.UserId = userId;
        }

        /// <summary>
        /// Gets the ID.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the RegionId.
        /// </summary>
        public Guid? RegionId { get; private set; }

        /// <summary>
        /// Gets the MirrorUrl.
        /// </summary>
        public string MirrorUrl { get; private set; }

        /// <summary>
        /// Gets the UserId.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Gets the ExpectedVersion.
        /// </summary>
        public int ExpectedVersion { get; private set; }
    }
}
