using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Library;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.IdentityAccess.Client.In;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpRecipientWriteRepository : IRecipientWriteRepository
    {
        private readonly IAvatarReadRepository avatarReadRepository;
        private readonly IPermitClient permitClient;
        private readonly ISettingsService settingsService;

        public HttpRecipientWriteRepository(
            IAvatarReadRepository avatarReadRepository,
            IPermitClient permitClient,
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(avatarReadRepository, nameof(avatarReadRepository));
            AssertionConcern.AssertArgumentNotNull(permitClient, nameof(permitClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.avatarReadRepository = avatarReadRepository;
            this.permitClient = permitClient;
            this.settingsService = settingsService;
        }
        
        public async Task SaveAll(IEnumerable<Recipient> recipients, string userId, CancellationToken token = default)
        {
            if (recipients.Any())
            { 
                var avatarsDict = (await this.avatarReadRepository.GetByIds(recipients.Select(d => d.AvatarId), userId))
                    .ToDictionary(r => r.Id);

                foreach (var r in recipients)
                {
                    var avatarNeuron = avatarsDict[r.AvatarId];

                    // TODO: Create recipient neurons

                    // grant access to avatar neurons / user neuron id
                    // TODO: should be done by a separate service or not at all since permissions should be based on regions
                    //await this.permitClient.CreateNeuronPermitAsync(
                    //    this.settingsService.IdentityAccessInBaseUrl + "/",
                    //    r.MessageId,
                    //    r.AvatarId,
                    //    DateTime.Now.AddDays(30),
                    //    token
                    //    );

                    #region 'Message Instance' 
                    // TODO: await this.transaction.CreatePropertyValue(
                    //    this.neuronAdapter,
                    //    this.terminalAdapter,
                    //    message.Id,
                    //    await this.libraryService.GetNeuronId(ExternalReferenceId.ReceiptInfo_MustHaveMessageInstance),
                    //    message.SenderId,
                    //    cpvdi
                    //    );
                    #endregion
                }
            }
        }
    }
}
