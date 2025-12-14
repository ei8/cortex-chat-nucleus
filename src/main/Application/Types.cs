namespace ei8.Cortex.Chat.Nucleus.Application
{
    /// <summary>
    /// Represents Application constants.
    /// </summary>
    public struct Constants
    {
        /// <summary>
        /// Represents Exception messages.
        /// </summary>
        public struct Exception
        {
            /// <summary>
            /// Exception message for Invalid UserId value.
            /// </summary>
            public const string InvalidUserId = "User Id must not be null or empty.";

            /// <summary>
            /// Exception message for Invalid ExceptionVersion.
            /// </summary>
            public const string InvalidExpectedVersion = "Expected Version must be equal to or greater than '1'.";
        }
    }
}
