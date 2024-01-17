using ei8.Cortex.Chat.Nucleus.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Owin;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.In.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
#if DEBUG
            services.AddHttpClient("ignoreSSL").ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (m, crt, chn, e) => true
                };
            });
#else
            services.AddHttpClient("ignoreSSL");
#endif
            services.AddSingleton(this.Configuration.GetSection("Authorities").Get<IEnumerable<Authority>>());
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(buildFunc => buildFunc.UseNancy(o => o.Bootstrapper = new CustomBootstrapper(app.ApplicationServices)));
        }
    }
}
