using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters;
using ei8.Cortex.Library.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.EnsembleServices
{
    public class Instantiates : IInstantiates
    {
        private readonly ICoreSet coreSet;
        private readonly IDependency dependency;

        public Instantiates(ICoreSet coreSet, IDependency dependency)
        {
            this.coreSet = coreSet;
            this.dependency = dependency;
        }

        public async Task Build(Neuron mainEnsemble, IEnumerable<Neuron> supplementaryEnsembles, IInstantiatesParameterSet parameterSet, INeuronRepository neuronRepository, string userId)
        {
            // TODO: check first if ensemble already contains classDirectObject before obtaining or
            // should obtainAsync accept ensemble so it can be done inside obtainAsync before retrieving from DB???
            var classDirectObject = await this.dependency.ObtainAsync(new DependencyParameterSet(
                    parameterSet.Class,
                    this.coreSet.DirectObject.CloneWithoutTerminals()
                ),
                neuronRepository,
                userId
                );

            #region Temp Code  - For Creating an 'Instantiates^[Class]~do' ensemble
            //var ers = await neuronService.GetExternalReferences(
            //    userId,
            //    ExternalReferenceKey.DirectObject,
            //    typeof(Domain.Model.Messages.Message),
            //    ExternalReferenceKey.InstantiatesMessage,
            //    ExternalReferenceKey.Subordination,
            //    ExternalReferenceKey.Instantiates_Unit
            //    );

            //// TODO: use NeuronQueryClient to determine whether 'Message~do' already exists in DB
            //// if not yet in DB 
            //// ... use to create neuron in memory
            //// var message_do = neuronService.CreateTransient();
            //// message_do.Tag = "Message~do";

            //// ... use to create based on NeuronData
            //var message_do = new NeuronData()
            //    {
            //        Id = Guid.Parse("3748be0d-94aa-40bf-b209-cb359194886c"),
            //        Tag = "Message~do"
            //    }.ToEnsemble();

            //var links1 = await terminalService.GetOrCreateTerminalsIfNotExistsAsync(
            //    message_do,
            //    userId,
            //    ers[ExternalReferenceKey.DirectObject],
            //    ers[typeof(Domain.Model.Messages.Message)]
            //    );

            //var instantiates_message_do = ers[ExternalReferenceKey.InstantiatesMessage];

            //var links2 = await terminalService.GetOrCreateTerminalsIfNotExistsAsync(
            //    instantiates_message_do,
            //    userId,
            //    ers[ExternalReferenceKey.Subordination],
            //    ers[ExternalReferenceKey.Instantiates_Unit],
            //    message_do
            //    );

            //var sd = instantiates_message_do.ToEnsembleData();

            ////await transaction.SaveEnsembleDataAsync(this.serviceProvider, sd, message.SenderId);
            #endregion

            return; // Task.CompletedTask;
        }

        public IEnumerable<Library.Common.NeuronQuery> GetQueries(IInstantiatesParameterSet parameterSet) =>
            new[] {
                new NeuronQuery()
                {
                    Id = new[] { parameterSet.Class.Id.ToString() },
                    DirectionValues = DirectionValues.Any,
                    Depth = 3,
                    TraversalMinimumDepthPostsynaptic = new[] {
                        new DepthIdsPair {
                            Depth = 3,
                            Ids = new[] {
                                this.coreSet.Subordination.Id,
                                this.coreSet.InstantiatesUnit.Id
                            }
                        }
                    }
                }
            };

        public bool TryParse(Neuron ensemble, IInstantiatesParameterSet parameterSet, out Neuron result)
        {
            result = null;
            IEnumerable<Neuron> paths = new[] { ensemble.Find(parameterSet.Class.Id) };

            var levelParsers = new LevelParser[]
            {
                new LevelParser(new PresynapticBySibling(this.coreSet.DirectObject.Id)),
                new LevelParser(new PresynapticBySibling(
                    this.coreSet.Subordination.Id,
                    this.coreSet.InstantiatesUnit.Id
                    ))
            };

            foreach (var levelParser in levelParsers)
                paths = levelParser.Evaluate(paths);

            if (paths.Count() == 1)
                result = paths.ToArray()[0];

            return result != null;

        }
    }
}
