using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    // TODO:0 Promote to appropriate assembly
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WriteRepositoryBase<T> where T : class
    {
        private readonly ITransaction transaction;
        private readonly INetworkTransactionService networkTransactionService;
        private readonly IneurULizer neurULizer;

        /// <summary>
        /// Constructs a WriteRepositoryBase.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="networkTransactionService"></param>
        /// <param name="neurULizer"></param>
        protected WriteRepositoryBase(
            ITransaction transaction,
            INetworkTransactionService networkTransactionService,
            IneurULizer neurULizer
        )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(networkTransactionService, nameof(networkTransactionService));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));

            this.transaction = transaction;
            this.networkTransactionService = networkTransactionService;
            this.neurULizer = neurULizer;
        }

        /// <summary>
        /// Saves the specified instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task Save(T instance, CancellationToken token = default)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var me = await this.neurULizer.neurULizeAsync(
                instance,
                token
            );

            watch.Stop();
            System.Diagnostics.Trace.WriteLine($"'{typeof(T).Name}' neurULization (secs): {watch.Elapsed.TotalSeconds}");
            watch.Restart();

            await this.networkTransactionService.SaveAsync(this.transaction, me);

            watch.Stop();
            System.Diagnostics.Trace.WriteLine($"Network save (secs): {watch.Elapsed.TotalSeconds}");
        }

        /// <summary>
        /// Saves all specified instances.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task SaveAll(IEnumerable<T> instances, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(instances, nameof(instances));
            AssertionConcern.AssertArgumentValid(
                s => s.Count() > 0,
                instances,
                $"At least one '{typeof(T).Name}' is required.",
                nameof(instances)
            );

            foreach (var i in instances)
                await this.Save(i, token);
        }
    }
}
