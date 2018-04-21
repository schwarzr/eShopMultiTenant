using ApplicationCore.Interfaces;
using ApplicationCore.Services;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Logging;
using Infrastructure.Services;
using Infrastructure.Tenant;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Interfaces;
using Microsoft.eShopWeb.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Text;

namespace Microsoft.eShopWeb
{
    public class Startup
    {
        private IServiceCollection _services;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env)
        {
            app.UseAuthentication();

            app.UseMiddleware<TenantMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                ListAllRegisteredServices(app);
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Catalog/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc();
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            // use in-memory database
            //ConfigureTestingServices(services);

            // use real database
            ConfigureProductionServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            // use real database
            services.AddDbContext<CatalogContext>((sp, c) =>
                c.UseSqlServer(sp.GetService<ITenantScope>().ConnectionString));

            ConfigureServices(services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("webapp");

                    options.ResponseType = "code";
                    options.RequireHttpsMetadata = false;
                    options.Authority = "http://localhost:8000";
                    options.ClientId = "12345678";
                    options.ClientSecret = "12345678";
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ClaimActions.Add(new JsonKeyClaimAction(ClaimTypes.Name, ClaimValueTypes.String, "name"));
                    options.ClaimActions.Add(new JsonKeyClaimAction("connectionstring", ClaimValueTypes.String, "connection_string"));
                    options.ClaimActions.Add(new JsonKeyClaimAction("tenantkey", ClaimValueTypes.String, "tenant_key"));
                    options.ClaimActions.Add(new JsonKeyClaimAction("tenantid", ClaimValueTypes.String, "tenant_id"));
                });

            services.AddSingleton<TenantCache>();
            services.AddScoped<ITenantScope, TenantScope>();

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));

            //services.AddScoped<ICatalogService, CachedCatalogService>();
            services.AddScoped<ICatalogService, CatalogService>();
            services.AddScoped<IBasketService, BasketService>();
            services.AddScoped<IBasketViewModelService, BasketViewModelService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<CatalogService>();
            services.Configure<CatalogSettings>(Configuration);
            services.AddSingleton<IUriComposer>(new UriComposer(Configuration.Get<CatalogSettings>()));

            services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
            services.AddTransient<IEmailSender, EmailSender>();

            // Add memory cache services
            services.AddMemoryCache();

            services.AddMvc();

            _services = services;
        }

        public void ConfigureTestingServices(IServiceCollection services)
        {
            // use in-memory database
            services.AddDbContext<CatalogContext>(c =>
                c.UseInMemoryDatabase("Catalog"));

            // Add Identity DbContext
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseInMemoryDatabase("Identity"));

            ConfigureServices(services);
        }

        private void ListAllRegisteredServices(IApplicationBuilder app)
        {
            app.Map("/allservices", builder => builder.Run(async context =>
            {
                var sb = new StringBuilder();
                sb.Append("<h1>All Services</h1>");
                sb.Append("<table><thead>");
                sb.Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>");
                sb.Append("</thead><tbody>");
                foreach (var svc in _services)
                {
                    sb.Append("<tr>");
                    sb.Append($"<td>{svc.ServiceType.FullName}</td>");
                    sb.Append($"<td>{svc.Lifetime}</td>");
                    sb.Append($"<td>{svc.ImplementationType?.FullName}</td>");
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
                await context.Response.WriteAsync(sb.ToString());
            }));
        }
    }
}