using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Infrastructure.Tenant.Migrations
{
    public partial class initialTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantSetup",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ConnectionString = table.Column<string>(maxLength: 500, nullable: false),
                    TenantKey = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSetup", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_TenantKey_Unique",
                table: "TenantSetup",
                column: "TenantKey",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantSetup");
        }
    }
}
