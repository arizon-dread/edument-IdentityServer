using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServerHost.Quickstart.UI;
using IdentityServerInMem;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityService
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _environment = environment;
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHsts(opts =>
            {
                opts.IncludeSubDomains = true;
                opts.MaxAge = TimeSpan.FromSeconds(15768000);
            });
            services.AddControllersWithViews();

            var builder = services.AddIdentityServer(options =>
            {

            }).AddTestUsers(TestUsers.Users)
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients);
            builder.AddDeveloperSigningCredential();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            } else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseSecurityHeaders();

            app.UseRouting();
           
            app.UseRequestLocalization(
                new RequestLocalizationOptions()
                .SetDefaultCulture("se-SE"));

            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
            
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        string build = "Debug build";
            //        if (Settings.IsReleaseBuild)
            //        {
            //            build = "Release build";
            //        }

            //        //Print out the first 8 characters of the GitHub SHA when deploying to production
            //        //Should of course be a bit more hidden in real life, perhaps as a HTML comment?
            //        var gitHubSha = _configuration["GITHUB:SHA"] ?? "";
            //        if (gitHubSha.Length > 8)
            //        {
            //            gitHubSha = " " + gitHubSha.Substring(0, 8);
            //        }

            //        //In real life this Should of course be a bit more hidden, perhaps as a HTML comment?
            //        await context.Response.WriteAsync($"Hello Identity Service!, Deployed {Infrastructure.Settings.StartupTime} ({env.EnvironmentName}, {build}{gitHubSha})");
            //    });
            //});
        }
    }
}
