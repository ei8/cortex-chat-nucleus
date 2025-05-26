using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageWriteRepository : IMessageWriteRepository
    {
        private readonly ITransaction transaction;
        private readonly INetworkTransactionService networkTransactionService;
        private readonly IneurULizer neurULizer;

        public HttpMessageWriteRepository(
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

        public async Task Save(Message message, CancellationToken token = default)
        {
            // TODO: handle updates - message.Version == 0 ? WriteMode.Create : WriteMode.Update
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var me = await this.neurULizer.neurULizeAsync(
                message,
                token
            );
            
            watch.Stop();
            System.Diagnostics.Debug.WriteLine($"neurULization (secs): {watch.Elapsed.TotalSeconds}");
            watch.Restart();

            await this.networkTransactionService.SaveAsync(this.transaction, me);

            watch.Stop();
            System.Diagnostics.Debug.WriteLine($"Network save (secs): {watch.Elapsed.TotalSeconds}");
        }
    }
}
