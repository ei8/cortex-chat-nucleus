using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Client.In;
using ei8.Cortex.Chat.Nucleus.Domain.Model;
using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using ei8.Cortex.Library.Client.Out;
using ei8.Cortex.Library.Common;
using IdentityModel.Client;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class HttpDestinationWriteRepository : IDestinationWriteRepository
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly INeuronQueryClient neuronQueryClient;
        private readonly IMessageClient messageClient;
        private readonly IIdentityService identityService;
        private readonly ISettingsService settingsService;

        public HttpDestinationWriteRepository(
            IHttpClientFactory httpClientFactory, 
            INeuronQueryClient neuronQueryClient, 
            IMessageClient messageClient,
            IIdentityService identityService,
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(httpClientFactory, nameof(httpClientFactory));
            AssertionConcern.AssertArgumentNotNull(neuronQueryClient, nameof(neuronQueryClient));
            AssertionConcern.AssertArgumentNotNull(messageClient, nameof(messageClient));
            AssertionConcern.AssertArgumentNotNull(identityService, nameof(identityService));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.httpClientFactory = httpClientFactory;
            this.neuronQueryClient = neuronQueryClient;
            this.messageClient = messageClient;
            this.identityService = identityService;
            this.settingsService = settingsService;
        }

        public async Task SaveAll(IEnumerable<Destination> destinations, CancellationToken token = default)
        {
            if (destinations.Any())
            {
                var client = this.httpClientFactory.CreateClient("ignoreSSL");
                var currentAvatarUrl = string.Empty;
                foreach (var d in destinations)
                {
                    // TODO: transfer region retrieval to a repository?
                    var regionNeuron = (
                            await this.neuronQueryClient.GetNeuronsInternal(
                                this.settingsService.CortexLibraryOutBaseUrl + "/",
                                new NeuronQuery()
                                {
                                    Id = new string[] { d.RegionId.ToString() },
                                    SortBy = SortByValue.NeuronCreationTimestamp,
                                    SortOrder = SortOrderValue.Descending
                                },
                                this.identityService.UserId
                            )
                        ).Items.SingleOrDefault();

                    if (string.IsNullOrWhiteSpace(currentAvatarUrl) && regionNeuron != null)
                    {
                        new Uri(regionNeuron.Url).ExtractParts(out string au, out string regionId);
                        currentAvatarUrl = au;
                    }

                    if (!string.IsNullOrWhiteSpace(regionNeuron.ExternalReferenceUrl))
                    {
                        new Uri(regionNeuron.ExternalReferenceUrl).ExtractParts(out string externalAvatarUrl, out string externalRegionId);

                        var authority = this.settingsService.Authorities.SingleOrDefault(au => au.Avatars.SingleOrDefault(av => av == externalAvatarUrl) != null);

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
                                await this.messageClient.CreateMessage(
                                    externalAvatarUrl + "/",
                                    Guid.NewGuid().ToString(),
                                    d.Message.Content,
                                    externalRegionId,
                                    $"{currentAvatarUrl}/cortex/neurons/{d.Message.Id}",
                                    null,
                                    response.AccessToken
                                    );
                            }
                            catch (Exception ex)
                            {
                                var e = ex;
                                // TODO: log any error while invoking destination
                            }
                        }
                        else
                        {
                            // TODO: log if authority for avatarurl was not found
                        }
                    }
                    else
                    {
                        // TODO: log exrefurl of local version of remote region was not found
                    }
                }
            }
        }
    }
}
