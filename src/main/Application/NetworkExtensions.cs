using ei8.Cortex.Coding;
using System;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    // TODO: transfer to ei8.Cortex.Coding
    public static class NetworkExtensions
    {
        public static T GetValidateNeuron<T>(this Network value, Guid id, Func<Neuron, T> neuronProcessor)
        {
            if (value.TryGetById(id, out Neuron n))
                return neuronProcessor(n);
            else
                throw new InvalidOperationException($"Neuron with Id '{id}' not found in 'readNetworkCache'.");
        }

        public static async Task<T> GetValidateNeuronAsync<T>(this Network value, Guid id, Func<Neuron, Task<T>> neuronProcessor)
        {
            if (value.TryGetById(id, out Neuron n))
                return await neuronProcessor(n);
            else
                throw new InvalidOperationException($"Neuron with Id '{id}' not found in 'readNetworkCache'.");
        }
    }
}
