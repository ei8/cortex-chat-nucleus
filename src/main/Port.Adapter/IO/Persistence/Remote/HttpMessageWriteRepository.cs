using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
using ei8.EventSourcing.Client;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    /// <summary>
    /// Represents a Message (write-only) Repository.
    /// </summary>
    public class HttpMessageWriteRepository : WriteRepositoryBase<Message>, IMessageWriteRepository
    {
        /// <summary>
        /// Constructs a Message Repository.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="networkTransactionService"></param>
        /// <param name="neurULizer"></param>
        public HttpMessageWriteRepository(
            ITransaction transaction,
            INetworkTransactionService networkTransactionService,
            IneurULizer neurULizer
        ) : base(
            transaction,
            networkTransactionService,
            neurULizer
        )
        {
        }
    }
}
