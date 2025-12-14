namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    /// <summary>
    /// Represents Domain constants.
    /// </summary>
    public struct Constants
    {
        /// <summary>
        /// Represents Exception messages.
        /// </summary>
        public struct Exception
        {
            /// <summary>
            /// Exception message for Invalid ID value.
            /// </summary>
            public const string InvalidId = "Id must not be equal to '00000000-0000-0000-0000-000000000000'.";
        }
    }
}
