using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Common
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDevHttpClient(this IServiceCollection services)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                services.AddHttpClient("ignoreSSL").ConfigurePrimaryHttpMessageHandler(() =>
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (m, crt, chn, e) => true
                    }
                );
            else
                services.AddHttpClient("ignoreSSL");
        }
    }
}
