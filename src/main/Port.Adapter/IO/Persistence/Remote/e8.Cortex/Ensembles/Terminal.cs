using neurUL.Cortex.Common;
using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class Terminal : IEnsembleItem
    {
        public Terminal(Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength) : this(id, false, presynapticNeuronId, postsynapticNeuronId, effect, strength)
        {
        }

        public Terminal(Guid id, bool isTransient, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength)
        {
            this.Id = id;
            this.IsTransient = isTransient;
            this.PresynapticNeuronId = presynapticNeuronId;
            this.PostsynapticNeuronId = postsynapticNeuronId;
            this.Effect = effect;
            this.Strength = strength;
        }

        public static Terminal CreateTransient(Guid presynapticNeuronId, Guid postsynapticNeuronId) => new Terminal(Guid.NewGuid(), true, presynapticNeuronId, postsynapticNeuronId, NeurotransmitterEffect.Excite, 1f);

        public Guid Id { get; private set; }
        public bool IsTransient { get; }
        public Guid PresynapticNeuronId { get; private set; }
        public Guid PostsynapticNeuronId { get; private set; }
        public NeurotransmitterEffect Effect { get; private set; }
        public float Strength { get; private set; }
    }
}
