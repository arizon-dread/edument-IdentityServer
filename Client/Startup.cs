using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Client
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
            services.AddControllersWithViews();
            services.AddHsts(opts =>
            {
                opts.IncludeSubDomains = true;
                opts.MaxAge = TimeSpan.FromSeconds(15768000);
            });
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                
            })
                .AddCookie(options =>
                {
                    options.LogoutPath = "/User/Logout";
                    options.AccessDeniedPath = "/User/AccessDenied";
                })
                .AddOpenIdConnect(options =>
                {
                    options.Authority = _configuration["openid:authority"];// "https://localhost:6001"; //"https://student3-identityservice.webapi.se/";
                    options.ClientId = _configuration["openid:clientid"];// "authcodeflowclient";
                    options.ClientSecret = _configuration["oidc-client-secret"];
                    options.ResponseType = "code";

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.AccessDeniedPath = "/User/AccessDenied";

                    options.Prompt = "consent";

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,
                    };
                });
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
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStatusCodePagesWithReExecute(pathFormat: "/error", queryFormat: "?error={0}");
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            
            app.UseStaticFiles();

            app.UseRouting();
            app.UseRequestLocalization(
                new RequestLocalizationOptions()
                .SetDefaultCulture("se-SE"));
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSecurityHeaders();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
