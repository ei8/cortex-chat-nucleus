using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.Filters;
using ei8.Cortex.Library.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.EnsembleServices
{
    public class Dependency : IDependency
    {
        private readonly ICoreSet coreSet;

        public Dependency(ICoreSet coreSet)
        {
            this.coreSet = coreSet;
        }

        
        public async Task Build(EnsembleCollection ensembles, IDependencyParameterSet parameterSet, INeuronRepository neuronRepository, string userId)
        {
            // the fact that this point is reached means that grandmother does not yet exist
            ensembles.TryFind(parameterSet.Value, out Neuron value, out int valueIndex);
            ensembles.TryFind(parameterSet.Type, out Neuron dependencyType, out int dependencyTypeIndex);
            Neuron dependency = Neuron.CreateTransient(string.Empty, null, null);


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
            return;
        }

        public IEnumerable<Library.Common.NeuronQuery> GetQueries(IDependencyParameterSet parameterSet) =>
            new[] {
                new NeuronQuery()
                {
                    Postsynaptic = new[] { 
                        parameterSet.Value.Id.ToString(),
                        parameterSet.Type.ToString()
                    },
                    DirectionValues = DirectionValues.Outbound,
                    Depth = 1
                }
            };

        public bool TryParse(Neuron ensemble, IDependencyParameterSet parameterSet, out Neuron result)
        {
            result = null;
            IEnumerable<Neuron> paths = new[] { ensemble.GetAllNeurons().Single(n => n.Id == parameterSet.Value.Id) };

            var levelParsers = new LevelParser[]
            {
                new LevelParser(new PresynapticBySibling(parameterSet.Type.Id)),
            };

            foreach (var levelParser in levelParsers)
                paths = levelParser.Evaluate(paths);

            if (paths.Count() == 1)
                result = paths.ToArray()[0];

            return result != null;

        }
    }
}
