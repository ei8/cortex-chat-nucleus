using ei8.Cortex.Coding;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    // TODO:0 Promote to appropriate assembly
    public interface IWriteCacheService
    {
        Task SaveAsync<T>(
            T instance, 
            CancellationToken cancellationToken, 
            Func<T, Neuron> neuronCreator, 
            Func<T, CancellationToken, Task> saver
        );

        Task SaveAllAsync<T>(
            IEnumerable<T> instances, 
            CancellationToken cancellationToken, 
            Func<T, Neuron> neuronCreator, 
            Func<IEnumerable<T>, 
            CancellationToken, Task> saver
        );
    }
}
