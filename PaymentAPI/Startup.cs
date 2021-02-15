using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Infrastructure;
using Infrastructure.DataProtection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using PaymentAPI.Middleware;
using Serilog;

namespace PaymentAPI
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
        public void ConfigureServices(IServiceCollection services)
        {
            if (_environment.EnvironmentName != "Offline")
            {
                services.AddDataProtectionWithSqlServerForPaymentApi(_configuration);
            }
            services.AddHsts(opts =>
            {
                opts.IncludeSubDomains = true;
                opts.MaxAge = TimeSpan.FromSeconds(15768000);
            });

            services.AddControllersWithViews();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts => {
                    opts.Authority = _configuration["openid:authority"];
                    opts.Audience = "payment";
                    opts.MapInboundClaims = false;
                    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,
                    };
                    opts.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                    opts.IncludeErrorDetails = true;
                    opts.BackchannelHttpHandler = new BackChannelListener();
                    opts.BackchannelTimeout = TimeSpan.FromSeconds(5);
                }
            );
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            IdentityModelEventSource.Logger.LogLevel = System.Diagnostics.Tracing.EventLevel.Verbose;
            var listener = new IdentityModelEventListener();
            //shows info about access tokens and other stuff. Is seen when the above EventLevel is set to Verbose.
            //IdentityModelEventSource.ShowPII = true;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseSerilogRequestLogging();
            app.UseWaitForIdentityServer(new WaitForIdentityServerOptions()
            { Authority = _configuration["openid:authority"] });
            app.UseHttpsRedirection();
            app.UseSecurityHeaders();
            app.UseRouting();
            app.UseRequestLocalization(
                new RequestLocalizationOptions()
                .SetDefaultCulture("se-SE"));
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            });
        }
    }
}
