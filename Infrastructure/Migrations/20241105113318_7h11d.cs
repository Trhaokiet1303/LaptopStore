using Microsoft.EntityFrameworkCore.Migrations;

namespace LaptopStore.Client.Migrations
{
    public partial class _7h11d : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "Brands");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Brands",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "Brands",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
