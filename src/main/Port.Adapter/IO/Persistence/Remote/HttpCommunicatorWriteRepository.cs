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
    public class HttpCommunicatorWriteRepository<T> : 
        WriteRepositoryBase<T>,
        ICommunicatorWriteRepository<T> 
        where T : CommunicatorBase, new()
    {
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
        ) : base(
            transaction,
            networkTransactionService,
            neurULizer
        )
        {
        }
    }
}
