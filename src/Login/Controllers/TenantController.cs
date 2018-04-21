using ApplicationCore.Interfaces;
using IdentityServer4;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ViewModels.Account;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Web.ViewModels.Account;

namespace Microsoft.eShopWeb.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public class TenantController : Controller
    {
        private readonly AppIdentityDbContext _db;

        public TenantController(
            AppIdentityDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signin(string returnUrl, Guid TenantId)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return await ProcessSignIn(returnUrl, TenantId);
        }

        [HttpGet]
        public async Task<IActionResult> Signin(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return await ProcessSignIn(returnUrl);
        }

        private async Task<IActionResult> ProcessSignIn(string returnUrl, Guid? tenantId = null)
        {
            var identity = this.User.Identity as ClaimsIdentity;

            var userId = identity.FindFirst("sub").Value;

            var tenantQuery = _db.TenantUsers.Where(p => p.User.Id == userId);

            if (tenantId.HasValue)
            {
                tenantQuery = tenantQuery.Where(p => p.TenantId == tenantId.Value);
            }

            var tenantInfos = await tenantQuery
                .Select(p => p.Tenant)
                .ToListAsync();

            if (!tenantInfos.Any())
            {
                return View("NoTenantAssigned");
            }

            if (tenantInfos.Count == 1)
            {
                identity = identity.Clone();
                identity.AddClaim(new Claim("connection_string", tenantInfos[0].ConnectionString));
                identity.AddClaim(new Claim("tenant_id", tenantInfos[0].Id.ToString()));
                identity.AddClaim(new Claim("tenant_key", tenantInfos[0].TenantKey));
            }
            else
            {
                return View(tenantInfos);
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Redirect(returnUrl);
        }
    }
}