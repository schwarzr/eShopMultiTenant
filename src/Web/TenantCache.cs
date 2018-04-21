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

        public async Task<bool?> GetTenantScopeAsync(string key, IServiceProvider sp)
        {
            return await _cache.GetOrCreateAsync<bool?>(key, p => CreateTenantScope(p, sp));
        }

        private async Task<bool?> CreateTenantScope(ICacheEntry arg, IServiceProvider sp)
        {
            var key = (string)arg.Key;

            var tenantScope = sp.GetRequiredService<ITenantScope>();

            var catalogContext = sp.GetRequiredService<CatalogContext>();
            var pending = await catalogContext.Database.GetPendingMigrationsAsync();

            if (pending.Any())
            {
                await catalogContext.Database.MigrateAsync();

                if (pending.Contains("20171018175735_Initial"))
                {
                    await CatalogContextSeed.SeedAsync(catalogContext, sp.GetRequiredService<ILoggerFactory>());
                }
            }

            return true;
        }
    }
}