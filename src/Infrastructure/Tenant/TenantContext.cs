using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tenant
{
    public class TenantContext : DbContext
    {
        public TenantContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TenantSetup>().HasIndex(p => p.TenantKey)
                .IsUnique()
                .HasName("IX_Tenant_TenantKey_Unique");
        }

        public DbSet<TenantSetup> TenantSetup { get; set; }
    }
}
