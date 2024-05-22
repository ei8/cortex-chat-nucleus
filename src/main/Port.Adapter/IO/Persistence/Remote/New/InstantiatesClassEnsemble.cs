using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public class InstantiatesClassEnsemble : IEnsemble
    {
        private readonly Guid directObjectId;
        private readonly Guid subordinationId;
        private readonly Guid instantiatesUnitId;

        public InstantiatesClassEnsemble(
            Guid directObjectId, 
            Guid subordinationId,
            Guid instantiatesUnitId
            )
        {
            this.directObjectId = directObjectId;
            this.subordinationId = subordinationId;
            this.instantiatesUnitId = instantiatesUnitId;
        }

        public IEnumerable<LevelParser> LevelEvaluators => new LevelParser[]
        {
            new LevelParser(new PresynapticBySibling(this.directObjectId)),
            new LevelParser(new PresynapticBySibling(
                this.subordinationId,
                this.instantiatesUnitId
                ))
        };

        public bool TryGet(Neuron value, out Neuron result)
        {
            result = null;
            List<Neuron> paths = new List<Neuron> { value };

            foreach (var levelEvaluator in LevelEvaluators)
            {
                var temp = levelEvaluator.Evaluate(paths);
                paths.Clear();
                paths.AddRange(temp);
            }

            if (paths.Count == 1)
                result = paths[0];

            return result != null;
        }
    }
}
