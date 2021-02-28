using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pocosearch.Example.Models;

namespace Pocosearch.Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Uncomment to use the embedded Elasticsearch setup:
            /*
            services.AddSingleton<EmbeddedSearchEngine>(EmbeddedSearchEngine.Launch());
            services.AddTransient<PocosearchClient>(
                x => new PocosearchClient(x.GetRequiredService<EmbeddedSearchEngine>()));
            */

            services.AddTransient<PocosearchClient>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, PocosearchClient pocosearch)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var options = new RewriteOptions()
                .AddRewrite("^$", "index.html", skipRemainingRules: true);

            app.UseRewriter(options);

            app.UseStaticFiles();

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            pocosearch.SetupIndex<Article>();
        }
    }
}
