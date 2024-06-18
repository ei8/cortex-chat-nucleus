using ei8.EventSourcing.Client;
using Microsoft.Extensions.DependencyInjection;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public static class TransactionExtensionMethods
    {
        public static async Task CreateUnitAsync(
            this IServiceProvider serviceProvider,
            Guid neuronId,
            TerminalIdPair unitTerminalIdPair,
            TerminalIdPair ideaTerminalIdPair,
            Guid authorId
            )
        {
            var transaction = serviceProvider.GetRequiredService<ITransaction>();
            var neuronAdapter = serviceProvider.GetRequiredService<INeuronAdapter>();
            var terminalAdapter = serviceProvider.GetRequiredService<ITerminalAdapter>();

            await transaction.InvokeAdapterAsync(
                neuronId,
                typeof(NeuronCreated).Assembly.GetEventTypes(),
                async (ev) => await neuronAdapter.CreateNeuron(
                    neuronId,
                    authorId
                )
            );

            await transaction.InvokeAdapterAsync(
                unitTerminalIdPair.TerminalId,
                typeof(TerminalCreated).Assembly.GetEventTypes(),
                async (ev) => await terminalAdapter.CreateTerminal(
                    unitTerminalIdPair.TerminalId,
                    neuronId,
                    unitTerminalIdPair.PostsynapticNeuronId,
                    neurUL.Cortex.Common.NeurotransmitterEffect.Excite,
                    1f,
                    authorId
                )
            );

            await transaction.InvokeAdapterAsync(
                ideaTerminalIdPair.TerminalId,
                typeof(TerminalCreated).Assembly.GetEventTypes(),
                async (ev) => await terminalAdapter.CreateTerminal(
                    ideaTerminalIdPair.TerminalId,
                    neuronId,
                    ideaTerminalIdPair.PostsynapticNeuronId,
                    neurUL.Cortex.Common.NeurotransmitterEffect.Excite,
                    1f,
                    authorId
                )
            );
        }

        public static async Task CreateSubordinationAsync(
            this IServiceProvider serviceProvider,
            Guid neuronId,
            TerminalIdPair subordinationTerminalIdPair,
            TerminalIdPair headTerminalIdPair,
            TerminalIdPair[] dependentTerminalIdPairs,
            Guid authorId
            )
        {
            var transaction = serviceProvider.GetRequiredService<ITransaction>();
            var neuronAdapter = serviceProvider.GetRequiredService<INeuronAdapter>();
            var terminalAdapter = serviceProvider.GetRequiredService<ITerminalAdapter>();

            await transaction.InvokeAdapterAsync(
                neuronId,
                typeof(NeuronCreated).Assembly.GetEventTypes(),
                async (ev) => await neuronAdapter.CreateNeuron(
                    neuronId,
                    authorId
                )
            );

            await transaction.InvokeAdapterAsync(
                subordinationTerminalIdPair.TerminalId,
                typeof(TerminalCreated).Assembly.GetEventTypes(),
                async (ev) => await terminalAdapter.CreateTerminal(
                    subordinationTerminalIdPair.TerminalId,
                    neuronId,
                    subordinationTerminalIdPair.PostsynapticNeuronId,
                    neurUL.Cortex.Common.NeurotransmitterEffect.Excite,
                    1f,
                    authorId
                )
            );

            await transaction.InvokeAdapterAsync(
                headTerminalIdPair.TerminalId,
                typeof(TerminalCreated).Assembly.GetEventTypes(),
                async (ev) => await terminalAdapter.CreateTerminal(
                    headTerminalIdPair.TerminalId,
                    neuronId,
                    headTerminalIdPair.PostsynapticNeuronId,
                    neurUL.Cortex.Common.NeurotransmitterEffect.Excite,
                    1f,
                    authorId
                )
            );

            foreach (var dti in dependentTerminalIdPairs)
            {
                await transaction.InvokeAdapterAsync(
                    dti.TerminalId,
                    typeof(TerminalCreated).Assembly.GetEventTypes(),
                    async (ev) => await terminalAdapter.CreateTerminal(
                        dti.TerminalId,
                        neuronId,
                        dti.PostsynapticNeuronId,
                        neurUL.Cortex.Common.NeurotransmitterEffect.Excite,
                        1f,
                        authorId
                    )
                );
            }
        }

        public static async Task CreateInstanceAsync(
            this IServiceProvider serviceProvider,
            Guid instanceNeuronId,
            string tag,
            Guid? regionId,
            string externalReferenceUrl,
            Guid instantiatesNeuronId,
            Guid authorId
            )
        {
            var transaction = serviceProvider.GetRequiredService<ITransaction>();
            var neuronAdapter = serviceProvider.GetRequiredService<INeuronAdapter>();
            var tagItemAdapter = serviceProvider.GetRequiredService<Data.Tag.Port.Adapter.In.InProcess.IItemAdapter>();
            var aggregateItemAdapter = serviceProvider.GetRequiredService<Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter>();
            var externalReferenceItemAdapter = serviceProvider.GetRequiredService<Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter>();
            var terminalAdapter = serviceProvider.GetRequiredService<ITerminalAdapter>();

            #region Create instance neuron
            int expectedVersion = await transaction.InvokeAdapterAsync(
                    instanceNeuronId,
                    typeof(NeuronCreated).Assembly.GetEventTypes(),
                    async (ev) => await neuronAdapter.CreateNeuron(
                        instanceNeuronId,
                        authorId)
                    );

            // assign tag value
            expectedVersion = await transaction.InvokeAdapterAsync(
                instanceNeuronId,
                typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly.GetEventTypes(),
                async (ev) => await tagItemAdapter.ChangeTag(
                    instanceNeuronId,
                    tag,
                    authorId,
                    ev
                ),
                expectedVersion
                );

            if (regionId.HasValue)
            {
                // assign region value to id
                expectedVersion = await transaction.InvokeAdapterAsync(
                    instanceNeuronId,
                    typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly.GetEventTypes(),
                    async (ev) => await aggregateItemAdapter.ChangeAggregate(
                        instanceNeuronId,
                        regionId.ToString(),
                        authorId,
                        ev
                    ),
                    expectedVersion
                );
            }

            if (!string.IsNullOrWhiteSpace(externalReferenceUrl))
            {
                expectedVersion = await transaction.InvokeAdapterAsync(
                    instanceNeuronId,
                    typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly.GetEventTypes(),
                    async (ev) => await externalReferenceItemAdapter.ChangeUrl(
                        instanceNeuronId,
                        externalReferenceUrl,
                        authorId,
                        ev
                    ),
                    expectedVersion
                    );
            }
            #endregion

            #region Create Instantiates terminal
            var instantiatesTerminalId = Guid.NewGuid();
            await transaction.InvokeAdapterAsync(
                instantiatesTerminalId,
                typeof(TerminalCreated).Assembly.GetEventTypes(),
                async (ev) => await terminalAdapter.CreateTerminal(
                    instantiatesTerminalId,
                    instanceNeuronId,
                    instantiatesNeuronId,
                    neurUL.Cortex.Common.NeurotransmitterEffect.Excite,
                    1f,
                    authorId
                )
                );
            #endregion
        }

        public static async Task<Guid> CreatePropertyValueAsync(
            this IServiceProvider serviceProvider,
            Guid valueNeuronId,
            Guid propertyDefinitionNeuronId,
            Guid authorId,
            CreatePropertyValueDependencyIds createPropertyValueDependencyIds
            )
        {
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, valueNeuronId, $"Value cannot be equal to '{Guid.Empty}'", nameof(valueNeuronId));
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, propertyDefinitionNeuronId, $"Value cannot be equal to '{Guid.Empty}'", nameof(propertyDefinitionNeuronId));
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, authorId, $"Value cannot be equal to '{Guid.Empty}'", nameof(authorId));
            AssertionConcern.AssertArgumentNotNull(createPropertyValueDependencyIds, nameof(createPropertyValueDependencyIds));

            var idSet = new PropertyIdSet();
            #region ...value expression
            #region ...head
            await serviceProvider.CreateUnitAsync(
                idSet.Declaration.Head.NeuronId,
                new TerminalIdPair(
                    idSet.Declaration.Head.UnitTerminalId,
                    createPropertyValueDependencyIds.Unit
                ),
                new TerminalIdPair(
                    idSet.Declaration.Head.IdeaTerminalId,
                    valueNeuronId
                ),
                authorId
            );
            #endregion
            #region ...subordination
            await serviceProvider.CreateSubordinationAsync(
                idSet.Declaration.Subordination.NeuronId,
                new TerminalIdPair(
                    idSet.Declaration.Subordination.SubordinationTerminalId,
                    createPropertyValueDependencyIds.Subordination
                ),
                new TerminalIdPair(
                    idSet.Declaration.Subordination.HeadTerminalId,
                    idSet.Declaration.Head.NeuronId
                ),
                new TerminalIdPair[]
                {
                    new TerminalIdPair(
                        idSet.Declaration.Subordination.DependentTerminalIds[(int) DeclarationIdSet.SubordinationDependentIndex.Dependent1],
                        createPropertyValueDependencyIds.Of_Case
                    )
                },
                authorId
            );
            #endregion
            #endregion

            #region ...property assignment
            #region ...dependent
            await serviceProvider.CreateUnitAsync(
                idSet.Assignment.Dependent.NeuronId,
                new TerminalIdPair(
                    idSet.Assignment.Dependent.DependentTerminalId,
                    createPropertyValueDependencyIds.NominalModifier
                ),
                new TerminalIdPair(
                    idSet.Assignment.Dependent.IdeaTerminalId,
                    idSet.Declaration.Subordination.NeuronId
                ),
                authorId
            );
            #endregion
            #region ...subordination
            await serviceProvider.CreateSubordinationAsync(
                idSet.Assignment.Subordination.NeuronId,
                new TerminalIdPair(
                    idSet.Assignment.Subordination.SubordinationTerminalId,
                    createPropertyValueDependencyIds.Subordination
                ),
                new TerminalIdPair(
                    idSet.Assignment.Subordination.HeadTerminalId,
                    propertyDefinitionNeuronId
                ),
                new TerminalIdPair[]
                {
                    new TerminalIdPair(
                        idSet.Assignment.Subordination.DependentTerminalIds[(int) AssignmentIdSet.SubordinationDependentIndex.Dependent1],
                        idSet.Assignment.Dependent.NeuronId
                    )
                },
                authorId
            );
            #endregion
            #endregion

            #region ...property association
            #region ...dependent
            await serviceProvider.CreateUnitAsync(
                idSet.Association.Dependent.NeuronId,
                new TerminalIdPair(
                    idSet.Association.Dependent.DependentTerminalId,
                    createPropertyValueDependencyIds.DirectObject
                ),
                new TerminalIdPair(
                    idSet.Association.Dependent.IdeaTerminalId,
                    idSet.Assignment.Subordination.NeuronId
                ),
                authorId
            );
            #endregion
            #region ...subordination
            await serviceProvider.CreateSubordinationAsync(
                idSet.Association.Subordination.NeuronId,
                new TerminalIdPair(
                    idSet.Association.Subordination.SubordinationTerminalId,
                    createPropertyValueDependencyIds.Subordination
                ),
                new TerminalIdPair(
                    idSet.Association.Subordination.HeadTerminalId,
                    createPropertyValueDependencyIds.Has_Unit
                ),
                new TerminalIdPair[]
                {
                    new TerminalIdPair(
                        idSet.Association.Subordination.DependentTerminalIds[(int) AssociationIdSet.SubordinationDependentIndex.Dependent1],
                        idSet.Association.Dependent.NeuronId
                    )
                },
                authorId
            );
            #endregion
            #endregion

            return idSet.Association.Subordination.NeuronId;
        }

        public static async Task LinkInstancePropertyValuesAsync(
            this IServiceProvider serviceProvider,
            Guid instanceNeuronId,
            IEnumerable<Guid> propertyValueIds,
            Guid authorId
            )
        {
            var transaction = serviceProvider.GetRequiredService<ITransaction>();
            var terminalAdapter = serviceProvider.GetRequiredService<ITerminalAdapter>();

            foreach (Guid pvi in propertyValueIds)
            {
                var terminalId = Guid.NewGuid();
                await transaction.InvokeAdapterAsync(
                    terminalId,
                    typeof(TerminalCreated).Assembly.GetEventTypes(),
                    async (ev) => await terminalAdapter.CreateTerminal(
                        terminalId,
                        instanceNeuronId,
                        pvi,
                        neurUL.Cortex.Common.NeurotransmitterEffect.Excite,
                        1f,
                        authorId
                    )
                );
            }
        }
    }
}
