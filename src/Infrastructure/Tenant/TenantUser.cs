using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Identity;

namespace Infrastructure.Tenant
{
    public class TenantUser
    {
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public TenantSetup Tenant { get; set; }

        public Guid TenantId { get; set; }
    }
}
