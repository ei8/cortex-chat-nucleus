using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    // TODO:1 transfer somewhere more apropriate, cortex.chat.nucleus should not be creating Avatars(?)
    public class HttpAvatarWriteRepository : IAvatarWriteRepository
    {
        private readonly ITransaction transaction;
        private readonly INetworkTransactionService networkTransactionService;
        private readonly IneurULizer neurULizer;
        

        public HttpAvatarWriteRepository(
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

        public async Task Save(Avatar avatar, CancellationToken token = default)
        {
            // TODO: handle updates - message.Version == 0 ? WriteMode.Create : WriteMode.Update
            var av = await this.neurULizer.neurULizeAsync(avatar);
            await this.networkTransactionService.SaveAsync(this.transaction, av);
        }
    }
}
