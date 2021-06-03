using Microsoft.EntityFrameworkCore.Migrations;

namespace eCommerce_API_RST.Migrations
{
    public partial class addOrderSentToDpsOutput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OrderSent",
                table: "DpsOutput",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderSent",
                table: "DpsOutput");
        }
    }
}
