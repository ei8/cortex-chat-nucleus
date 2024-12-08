using ei8.Cortex.Chat.Nucleus.Domain.Model;
using System;
using System.Text.RegularExpressions;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public static class ExtensionMethods
    {
        internal static Avatar ToDomainAvatar(this Library.Common.Neuron value)
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

        public static string GetFullyQualifiedEnumName<T>(this T @this) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enum");
            }
            var type = typeof(T);
            return string.Format("{0}.{1}.{2}", type.Namespace, type.Name, Enum.GetName(type, @this));
        }        
    }
}
