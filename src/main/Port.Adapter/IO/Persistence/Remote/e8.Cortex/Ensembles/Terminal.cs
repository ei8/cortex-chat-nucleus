using neurUL.Cortex.Common;
using System;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class Terminal
    {
        public Terminal(Guid id, float strength, NeurotransmitterEffect effect) : this(id, false, strength, effect)
        {
        }

        public Terminal(Guid id, bool isTransient, float strength, NeurotransmitterEffect effect)
        {
            this.Id = id;
            this.IsTransient = isTransient;
            this.Strength = strength;
            this.Effect = effect;
        }

        public Terminal() : this(Guid.NewGuid(), true, 1f, NeurotransmitterEffect.Excite)
        {
        }

        public void Unlink()
        {
            this.Presynaptic.RemoveTerminal(this.Id);
            this.Postsynaptic.RemoveDendrite(this.Id);

            this.Presynaptic = null;
            this.Postsynaptic = null;
        }

        public void Link(Neuron presynaptic, Neuron postsynaptic)
        {
            this.Presynaptic = presynaptic;
            this.Postsynaptic = postsynaptic;
            presynaptic.AddTerminal(this);
            postsynaptic.AddDendrite(this);
        }

        public Guid Id { get; private set; }
        public bool IsTransient { get; private set; }
        public float Strength { get; private set; }
        public NeurotransmitterEffect Effect { get; private set; }
        public Neuron Presynaptic { get; private set; }
        public Neuron Postsynaptic { get; private set; }
    }
}
