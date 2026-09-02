using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantUserId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantUserId",
                table: "AspNetUsers");
        }
    }
}
