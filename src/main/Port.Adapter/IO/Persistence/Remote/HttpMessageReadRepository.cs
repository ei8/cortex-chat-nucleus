using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.Out;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using IdentityModel.Client;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpMessageReadRepository : IMessageReadRepository
    {
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly IMessageQueryClient messageQueryClient;
        private readonly ISettingsService settingsService;
        private readonly IHttpClientFactory httpClientFactory;

        public HttpMessageReadRepository(
            INeuronQueryClient neuronQueryClient,
            IMessageQueryClient messageQueryClient,
            ISettingsService settingsService,
            IHttpClientFactory httpClientFactory
            )
        {
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(messageQueryClient, nameof(messageQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(httpClientFactory, nameof(httpClientFactory));

            this.neuronQueryClient = neuronQueryClient;
            this.messageQueryClient = messageQueryClient;
            this.settingsService = settingsService;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<Message>> GetAll(DateTimeOffset? maxTimestamp, int? pageSize, string userId, CancellationToken token = default)
        {
            if (!maxTimestamp.HasValue)
                maxTimestamp = DateTimeOffset.UtcNow;

            if (!pageSize.HasValue)
                pageSize = this.settingsService.PageSize;

            var neurons = await this.neuronQueryClient.GetNeuronsInternal(
                this.settingsService.CortexLibraryOutBaseUrl + "/",
                new NeuronQuery()
                {
                    PostsynapticExternalReferenceUrl = new string[] { this.settingsService.InstantiatesMessageExternalReferenceUrl },
                    SortBy = SortByValue.NeuronCreationTimestamp,
                    SortOrder = SortOrderValue.Descending
                },
                userId
                );

            var result = neurons.Items
                .Where(nr => DateTimeOffset.TryParse(nr.Creation.Timestamp, out DateTimeOffset currentCreationTimestamp) && currentCreationTimestamp <= maxTimestamp)
                .Take(pageSize.Value)
                .Reverse()
                .Select(n => new Message()
                {
                    Id = Guid.Parse(n.Id),
                    Content = n.Tag,
                    Region = n.Region?.Tag,
                    RegionId = Guid.TryParse(n.Region?.Id, out Guid newRegionId) ? (Guid?)newRegionId : null,
                    Sender = n.Creation.Author.Tag,
                    SenderId = Guid.Parse(n.Creation.Author.Id),
                    CreationTimestamp = DateTimeOffset.TryParse(n.Creation.Timestamp, out DateTimeOffset creation) ? (DateTimeOffset?)creation : null,
                    UnifiedLastModificationTimestamp = 
                        DateTimeOffset.TryParse(
                            n.UnifiedLastModification.Timestamp, 
                            out DateTimeOffset unifiedLastModification) ? 
                                (DateTimeOffset?)unifiedLastModification : 
                                null,
                    IsCurrentUserCreationAuthor = n.Validation.IsCurrentUserCreationAuthor
                });

            var client = this.httpClientFactory.CreateClient("ignoreSSL");

            var avatarUrl = "http://192.168.1.110:65111";
            var authority = this.settingsService.Authorities.SingleOrDefault(au => au.Avatars.SingleOrDefault(av => av == avatarUrl) != null);

            if (authority != null)
            {
                var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = authority.Address + "/connect/token",
                    ClientId = authority.ClientId,
                    ClientSecret = authority.ClientSecret
                });

                try
                {
                    var remoteMessages = (await this.messageQueryClient.GetMessagesAsync(
                       avatarUrl + "/",
                        response.AccessToken
                        )).Select(md => md.ToDomain());

                    result = result.Concat(remoteMessages);
                }
                catch (Exception ex)
                {
                    var e = ex;
                }
            }
            else
            {
                // TODO: log if authority for avatarurl was not found
            }

            return result;
        }
    }
}
