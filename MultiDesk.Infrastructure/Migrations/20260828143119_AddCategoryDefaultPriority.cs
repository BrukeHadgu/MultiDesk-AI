using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryDefaultPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultPriority",
                table: "Categories",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPriority",
                table: "Categories");
        }
    }
}
