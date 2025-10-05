using ei8.Cortex.Coding;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Application
{
    // TODO:0 Promote to appropriate assembly
    public class WriteCacheService : IWriteCacheService
    {
        private readonly INetworkDictionary<CacheKey> readWriteCache;

        public WriteCacheService(INetworkDictionary<CacheKey> readWriteCache)
        {
            AssertionConcern.AssertArgumentNotNull(readWriteCache, nameof(readWriteCache));

            this.readWriteCache = readWriteCache;
        }
        public async Task SaveAsync<T>(T instance, CancellationToken cancellationToken, Func<T, Neuron> neuronCreator, Func<T, CancellationToken, Task> saver)
        {
            AssertionConcern.AssertArgumentNotNull(instance, nameof(instance));
            AssertionConcern.AssertArgumentNotNull(neuronCreator, nameof(neuronCreator));
            AssertionConcern.AssertArgumentNotNull(saver, nameof(saver));

            this.readWriteCache[CacheKey.Write].AddReplace(
                neuronCreator.Invoke(instance)
            );

            await saver(instance, cancellationToken);
        }

        public async Task SaveAllAsync<T>(IEnumerable<T> instances, CancellationToken cancellationToken, Func<T, Neuron> neuronCreator, Func<IEnumerable<T>, CancellationToken, Task> saver)
        {
            AssertionConcern.AssertArgumentNotNull(instances, nameof(instances));
            AssertionConcern.AssertArgumentNotNull(neuronCreator, nameof(neuronCreator));
            AssertionConcern.AssertArgumentNotNull(saver, nameof(saver));

            instances.ToList().ForEach(i =>
                this.readWriteCache[CacheKey.Write].AddReplace(
                    neuronCreator.Invoke(i)
                )
            );

            await saver(instances, cancellationToken);
        }
    }
}
