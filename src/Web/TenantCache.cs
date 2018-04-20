using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Tenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb
{
    public class TenantCache
    {
        private readonly MemoryCache _cache;

        public TenantCache()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public async Task<ITenantScope> GetTenantScopeAsync(string key, IServiceProvider sp)
        {
            return await _cache.GetOrCreateAsync<ITenantScope>(key, p => CreateTenantScope(p, sp));
        }

        private async Task<ITenantScope> CreateTenantScope(ICacheEntry arg, IServiceProvider sp)
        {
            var key = (string)arg.Key;

            var ctx = sp.GetRequiredService<TenantContext>();
            var data = await ctx.TenantSetup.SingleOrDefaultAsync(p => p.TenantKey == key);

            ITenantScope result = null;
            if (data != null)
            {
                result = new TenantScope
                {
                    TenantKey = data.TenantKey,
                    ConnectionString = data.ConnectionString
                };

                using (var scope = sp.CreateScope())
                {
                    var tenantScope = scope.ServiceProvider.GetRequiredService<ITenantScope>();
                    tenantScope.TenantKey = result.TenantKey;
                    tenantScope.ConnectionString = result.ConnectionString;

                    var catalogContext = scope.ServiceProvider.GetRequiredService<CatalogContext>();
                    var pending = await catalogContext.Database.GetPendingMigrationsAsync();

                    if (pending.Any())
                    {
                        await catalogContext.Database.MigrateAsync();
                        var identityContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
                        await identityContext.Database.MigrateAsync();

                        if (pending.Contains("20171018175735_Initial"))
                        {
                            await CatalogContextSeed.SeedAsync(catalogContext, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
                            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                            await AppIdentityDbContextSeed.SeedAsync(userManager);
                        }
                    }
                }

            }
            arg.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            return result;
        }
    }
}