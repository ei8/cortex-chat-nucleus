﻿using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Library.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    internal static class ExtensionMethods
    {
        internal static Avatar ToDomainAvatar(this Neuron value)
        {
            return new Avatar()
            {
                Id = Guid.Parse(value.Id),
                Name = value.Tag,
                ExternalReferenceUrl = value.ExternalReferenceUrl,
                Url = value.Url
            };
        }

        internal static void ExtractParts(this Uri neuronUrl, out string avatarUrl, out string id)
        {
            var match = Regex.Match(neuronUrl.AbsoluteUri, "(?<AvatarUrl>.*)\\/cortex\\/neurons\\/(?<Id>.*)?");
            avatarUrl = match.Groups["AvatarUrl"].Value;
            id = match.Groups["Id"].Value;
        }
    }
}
