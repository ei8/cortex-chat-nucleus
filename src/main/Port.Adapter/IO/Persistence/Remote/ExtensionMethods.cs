using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Library.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    internal static class ExtensionMethods
    {
        internal static Region ToRegion(this Neuron value)
        {
            return new Region()
            {
                Id = Guid.Parse(value.Id),
                Name = value.Tag,
                ExternalReferenceUrl = value.ExternalReferenceUrl
            };
        }

        internal static void ExtractParts(this Uri neuronUrl, out string avatarUrl, out string externalRegionId)
        {
            var match = Regex.Match(neuronUrl.AbsoluteUri, "(?<AvatarUrl>.*)\\/cortex\\/neurons\\/(?<ExternalRegionId>.*)?");
            avatarUrl = match.Groups["AvatarUrl"].Value;
            externalRegionId = match.Groups["ExternalRegionId"].Value;
        }
    }
}
