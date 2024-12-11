using Microsoft.EntityFrameworkCore.Migrations;

namespace LaptopStore.Client.Migrations
{
    public partial class isRated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRated",
                table: "OrderItem",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRated",
                table: "OrderItem");
        }
    }
}
