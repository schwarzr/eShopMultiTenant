using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace Login
{
    internal class TenantProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            foreach (var item in context.Subject.Claims)
            {
                context.IssuedClaims.Add(item);
            }

            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}