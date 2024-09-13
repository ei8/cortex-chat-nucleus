using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23;
using ei8.Cortex.Coding.d23.Grannies;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.EventSourcing.Client;
using Microsoft.Extensions.DependencyInjection;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageWriteRepository : IMessageWriteRepository
    {
        private readonly IServiceProvider serviceProvider;

        public HttpMessageWriteRepository(IServiceProvider serviceProvider)
        {
            AssertionConcern.AssertArgumentNotNull(serviceProvider, nameof(serviceProvider));
            
            this.serviceProvider = serviceProvider;
        }

        public async Task Save(Message message, string userId, CancellationToken token = default)
        {
            var ensembleRepository = this.serviceProvider.GetRequiredService<IEnsembleRepository>();

            var me = await new neurULizer().neurULizeAsync(message, new d23neurULizerWriteOptions(
                await ensembleRepository.CreatePrimitives(userId),
                userId,
                new WriteOptions(
                    message.Version == 0 ?
                        WriteMode.Create :
                        WriteMode.Update
                ),
                this.serviceProvider.GetRequiredService<ei8.Cortex.Coding.d23.neurULization.Processors.Writers.IInstanceProcessor>(),
                this.serviceProvider.GetRequiredService<IEnsembleRepository>()
            ));

            await this.serviceProvider.GetRequiredService<ITransaction>()
                .SaveEnsembleAsync(this.serviceProvider, me, message.SenderId);
        }
    }
}
