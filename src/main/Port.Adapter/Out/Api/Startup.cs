using ei8.Cortex.Chat.Nucleus.Application;
using ei8.Cortex.Chat.Nucleus.Port.Adapter.Common;
using ei8.Cortex.Coding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Owin;
using System.Collections.Generic;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.Out.Api
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDevHttpClient();
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.AddOptions();
            services.Configure<List<MirrorConfig>>(configuration.GetSection("Mirrors"));
            services.Configure<List<Authority>>(configuration.GetSection("Authorities"));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(buildFunc => buildFunc.UseNancy(o => o.Bootstrapper = new CustomBootstrapper(app.ApplicationServices)));
        }
    }
}
