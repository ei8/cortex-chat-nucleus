using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.Cortex.Coding.d23.neurULization.Processors.Readers.Deductive;
using ei8.Cortex.Coding.d23.neurULization.Processors.Writers;
using ei8.EventSourcing.Client;
using Microsoft.Extensions.DependencyInjection;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    internal static class ExtensionMethods
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

        internal static async Task<PrimitiveSet> CreatePrimitives(this IEnsembleRepository ensembleRepository, string userId)
        {
            var refs = await ensembleRepository.GetExternalReferencesAsync(
                userId,
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

        public static async Task<IInstantiatesClass> GetInstantiatesClass(
            this IEnsembleRepository ensembleRepository,
            Id23neurULizerWriteOptions options, 
            Coding.Neuron @class
            )
        {
            var icProc = options.ServiceProvider.GetRequiredService<
                Coding.d23.neurULization.Processors.Readers.Deductive.IInstantiatesClassProcessor
                >();
            
            var icParams = new InstantiatesClassParameterSet(@class);
            var icPqs = icProc.GetQueries(
                options,
                icParams
            );
            var ensemble = new Ensemble();
            await icPqs.Process(
                new ProcessParameters(
                    ensemble,
                    options
                ),
                new List<IGranny>()
            );

            IInstantiatesClass instantiatesClass = null;
            AssertionConcern.AssertStateTrue(
                icProc.TryParse(
                    ensemble,
                    options,
                    icParams,
                    out instantiatesClass
                ),
                $"'{typeof(Coding.d23.Grannies.IInstantiatesClass).Name}' Granny is required to deneurULize 'Instantiates^{@class.Tag}'"
                );
            return instantiatesClass;
        }

        #region ITransaction
        // TODO: Transfer ei8.Cortex.Coding.EventSourcing or Data?
        public static async Task SaveEnsembleAsync(
           this ITransaction transaction,
           IServiceProvider serviceProvider,
           Ensemble ensemble,
           Guid authorId
           )
        {
            var transientItems = ensemble.GetItems().Where(ei => ei.IsTransient);
            foreach (var ei in transientItems)
                await transaction.SaveItemAsync(serviceProvider, ei, authorId);
        }

        public static async Task SaveItemAsync(
           this ITransaction transaction,
           IServiceProvider serviceProvider,
           IEnsembleItem item,
           Guid authorId
           )
        {
            if (item is Coding.Terminal terminal)
            {
                var terminalAdapter = serviceProvider.GetRequiredService<ITerminalAdapter>();

                await transaction.InvokeAdapterAsync(
                    terminal.Id,
                    typeof(TerminalCreated).Assembly.GetEventTypes(),
                    async (ev) => await terminalAdapter.CreateTerminal(
                        terminal.Id,
                        terminal.PresynapticNeuronId,
                        terminal.PostsynapticNeuronId,
                        (neurUL.Cortex.Common.NeurotransmitterEffect) Enum.Parse(typeof(neurUL.Cortex.Common.NeurotransmitterEffect), terminal.Effect.ToString()),
                        terminal.Strength,
                        authorId
                    )
                );
            }
            else if (item is Coding.Neuron neuron)
            {
                var neuronAdapter = serviceProvider.GetRequiredService<INeuronAdapter>();
                var tagItemAdapter = serviceProvider.GetRequiredService<Data.Tag.Port.Adapter.In.InProcess.IItemAdapter>();
                var aggregateItemAdapter = serviceProvider.GetRequiredService<Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter>();
                var externalReferenceItemAdapter = serviceProvider.GetRequiredService<Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter>();

                #region Create instance neuron
                int expectedVersion = await transaction.InvokeAdapterAsync(
                        neuron.Id,
                        typeof(NeuronCreated).Assembly.GetEventTypes(),
                        async (ev) => await neuronAdapter.CreateNeuron(
                            neuron.Id,
                            authorId)
                        );

                // assign tag value
                if (!string.IsNullOrWhiteSpace(neuron.Tag))
                {
                    expectedVersion = await transaction.InvokeAdapterAsync(
                        neuron.Id,
                        typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly.GetEventTypes(),
                        async (ev) => await tagItemAdapter.ChangeTag(
                            neuron.Id,
                            neuron.Tag,
                            authorId,
                            ev
                        ),
                        expectedVersion
                        );
                }

                if (neuron.RegionId.HasValue)
                {
                    // assign region value to id
                    expectedVersion = await transaction.InvokeAdapterAsync(
                        neuron.Id,
                        typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly.GetEventTypes(),
                        async (ev) => await aggregateItemAdapter.ChangeAggregate(
                            neuron.Id,
                            neuron.RegionId.ToString(),
                            authorId,
                            ev
                        ),
                        expectedVersion
                    );
                }

                if (!string.IsNullOrWhiteSpace(neuron.ExternalReferenceUrl))
                {
                    expectedVersion = await transaction.InvokeAdapterAsync(
                        neuron.Id,
                        typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly.GetEventTypes(),
                        async (ev) => await externalReferenceItemAdapter.ChangeUrl(
                            neuron.Id,
                            neuron.ExternalReferenceUrl,
                            authorId,
                            ev
                        ),
                        expectedVersion
                        );
                }
                #endregion
            }
        }
        #endregion
    }
}
