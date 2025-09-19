using neurUL.Common.Domain.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    public static class EnumerableExtensions
    {
        // TODO: Relocate to appropriate assembly
        /// <summary>
        /// Validates specified IDs are neither null, an empty array, nor contains duplicated values.
        /// </summary>
        /// <param name="ids"></param>
        public static void ValidateIds(this IEnumerable<Guid> ids)
        {
            AssertionConcern.AssertArgumentNotNull(ids, nameof(ids));
            AssertionConcern.AssertArgumentValid(i => i.Any(), ids, $"Specified value cannot be an empty array.", nameof(ids));
            AssertionConcern.AssertArgumentValid(i => i.Count() == ids.Distinct().Count(), ids, "Specified IDs should be distinct.", nameof(ids));
        }
    }
}
