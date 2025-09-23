using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Messages
{
    /// <summary>
    /// Represents a MessageQuery.
    /// </summary>
    public class MessageQuery
    {
        /// <summary>
        /// Constructs a MessageQuery using default parameter values;
        /// </summary>
        public MessageQuery() : this(null, null)
        {
        }

        /// <summary>
        /// Constructs a MessageQuery using the specified parameters.
        /// </summary>
        /// <param name="maxTimestamp"></param>
        /// <param name="pageSize"></param>
        public MessageQuery(DateTimeOffset? maxTimestamp, int? pageSize)
        {
            this.MaxTimestamp = maxTimestamp;
            this.PageSize = pageSize;
        }

        /// <summary>
        /// Gets or sets the oldest age of the Messages to be retrieved.
        /// </summary>
        public DateTimeOffset? MaxTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the number of Messages to be retrieved.
        /// </summary>
        public int? PageSize { get; set; }
    }
}
