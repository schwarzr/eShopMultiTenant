using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.eShopWeb
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var authorizeResult = await context.AuthenticateAsync();
            if (authorizeResult.Succeeded && authorizeResult.Principal != null)
            {
                var token = await context.GetTokenAsync("access_token");

                await _next(context);
            }
            else
            {
                await context.ChallengeAsync();
            }
            //var scope = context.RequestServices.GetService<ITenantScope>();

            //var segments = context.Request.Path.Value.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
            //if (!segments.Any())
            //{
            //    context.Response.StatusCode = StatusCodes.Status404NotFound;
            //    return;
            //}

            //var key = segments.First();
            //var cache = context.RequestServices.GetService<TenantCache>();
            //var cachedScope = await cache.GetTenantScopeAsync(key, context.RequestServices);

            //if (cachedScope == null)
            //{
            //    context.Response.StatusCode = StatusCodes.Status404NotFound;
            //    return;
            //}

            //scope.TenantKey = cachedScope.TenantKey;
            //scope.ConnectionString = cachedScope.ConnectionString;

            //context.Request.PathBase = context.Request.PathBase + "/" + scope.TenantKey;
            //context.Request.Path = "/" + string.Join("/", segments.Skip(1));


        }
    }
}