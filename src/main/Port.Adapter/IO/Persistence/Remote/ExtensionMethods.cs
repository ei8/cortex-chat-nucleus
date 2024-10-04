using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public static async Task<IPrimitiveSet> GetPrimitives(this IEnsembleRepository ensembleRepository)
        {
            var refs = await ensembleRepository.GetExternalReferencesAsync(
                new object[]
                {
                    ExternalReferenceKey.DirectObject,
                    ExternalReferenceKey.Idea,
                    ExternalReferenceKey.Instantiates,
                    ExternalReferenceKey.Simple,
                    ExternalReferenceKey.Subordination,
                    ExternalReferenceKey.Coordination,
                    ExternalReferenceKey.Unit,
                    ExternalReferenceKey.Of,
                    ExternalReferenceKey.Case,
                    ExternalReferenceKey.NominalModifier,
                    ExternalReferenceKey.Has
                }
            );

            return new PrimitiveSet()
            {
                DirectObject = refs[ExternalReferenceKey.DirectObject],
                Idea = refs[ExternalReferenceKey.Idea],
                Instantiates = refs[ExternalReferenceKey.Instantiates],
                Simple = refs[ExternalReferenceKey.Simple],
                Subordination = refs[ExternalReferenceKey.Subordination],
                Coordination = refs[ExternalReferenceKey.Coordination],
                Unit = refs[ExternalReferenceKey.Unit],
                Of = refs[ExternalReferenceKey.Of],
                Case = refs[ExternalReferenceKey.Case],
                NominalModifier = refs[ExternalReferenceKey.NominalModifier],
                Has = refs[ExternalReferenceKey.Has]
            };
        }
    }
}
