using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
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
    /// <summary>
    /// Represents a Communicator (write-only) Repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpCommunicatorWriteRepository<T> : ICommunicatorWriteRepository<T> where T : CommunicatorBase, new()
    {
        private readonly ITransaction transaction;
        private readonly INetworkTransactionService networkTransactionService;
        private readonly IneurULizer neurULizer;

        /// <summary>
        /// Constructs a Communicator Repository.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="networkTransactionService"></param>
        /// <param name="neurULizer"></param>
        public HttpCommunicatorWriteRepository(
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
        /// Saves all specified Communicator values.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SaveAll(IEnumerable<T> values, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(values, nameof(values));
            AssertionConcern.AssertArgumentValid(
                s => s.Count() > 0,
                values,
                $"At least one '{typeof(T).Name}' is required.",
                nameof(values)
            );

            foreach (var s in values)
            {
                var ns = await this.neurULizer.neurULizeAsync(s, token);
                await this.networkTransactionService.SaveAsync(this.transaction, ns);
            }
        }
    }
}
