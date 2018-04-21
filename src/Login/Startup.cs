using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Identity;
using Infrastructure.Tenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using ApplicationCore.Interfaces;
using Infrastructure.Logging;
using Infrastructure.Services;

namespace Login
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
             .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = "/Account/Signin";
                options.LogoutPath = "/Account/Signout";
            });

            services.AddMvc();

            // Add Identity DbContext
            services.AddDbContext<AppIdentityDbContext>((sp, options) =>
                options.UseSqlServer(Configuration.GetConnectionString("TenantContext")));

            services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = "~/account/signin";
            })
            .
            .AddDeveloperSigningCredential()
            .AddInMemoryPersistedGrants()
            .AddInMemoryIdentityResources(
                new IdentityResource[]
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Email(),
                    new IdentityResources.Profile()
                })
            .AddInMemoryApiResources(new[] {
                new ApiResource("webapp","eStore Web application",new []{ "connection_string", "tenant_id", "user_id" })
            })
            .AddInMemoryClients(new[]{
                new Client {
                    ClientId = "12345678",
                    ClientSecrets = new Secret[] { new Secret("12345678".Sha256()) },
                    ClientName = "Webapp",
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes =  {"openid", "profile", "webapp"},
                    AccessTokenType = AccessTokenType.Jwt,
                    RedirectUris = { "http://localhost:7000/signin-oidc"},
                    RequireConsent = false
                }
            })
            .AddAspNetIdentity<ApplicationUser>();


            services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
            services.AddTransient<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseIdentityServer();

            app.UseMvc();
        }
    }
}
