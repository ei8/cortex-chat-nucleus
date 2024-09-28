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
        private readonly IEnsembleTransactionService ensembleTransactionService;
        private readonly IneurULizer neurULizer;

        public HttpMessageWriteRepository(
            ITransaction transaction,
            IEnsembleTransactionService ensembleTransactionService,
            IneurULizer neurULizer
        )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(ensembleTransactionService, nameof(ensembleTransactionService));
            AssertionConcern.AssertArgumentNotNull(neurULizer, nameof(neurULizer));

            this.transaction = transaction;
            this.ensembleTransactionService = ensembleTransactionService;
            this.neurULizer = neurULizer;
        }

        public async Task Save(Message message, string userId, CancellationToken token = default)
        {
            // TODO: handle updates - message.Version == 0 ? WriteMode.Create : WriteMode.Update
            var me = await this.neurULizer.neurULizeAsync(message, userId);

            await this.ensembleTransactionService.SaveAsync(this.transaction, me, message.SenderId);
        }
    }
}
