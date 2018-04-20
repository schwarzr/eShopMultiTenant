using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Tenant
{
    public class TenantSetup
    {
        public Guid Id { get; set; }

        [StringLength(50)]
        [Required]
        public string TenantKey { get; set; }

        [StringLength(500)]
        [Required]
        public string ConnectionString { get; set; }
    }
}