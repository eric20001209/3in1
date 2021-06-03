using Microsoft.EntityFrameworkCore.Migrations;

namespace eCommerce_API_RST.Migrations.Freight
{
    public partial class addFreightrange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FreightRangeEnd1",
                table: "freight_settings",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightRangeEnd2",
                table: "freight_settings",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightRangeEnd3",
                table: "freight_settings",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightRangeStart1",
                table: "freight_settings",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightRangeStart2",
                table: "freight_settings",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightRangeStart3",
                table: "freight_settings",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreightRangeEnd1",
                table: "freight_settings");

            migrationBuilder.DropColumn(
                name: "FreightRangeEnd2",
                table: "freight_settings");

            migrationBuilder.DropColumn(
                name: "FreightRangeEnd3",
                table: "freight_settings");

            migrationBuilder.DropColumn(
                name: "FreightRangeStart1",
                table: "freight_settings");

            migrationBuilder.DropColumn(
                name: "FreightRangeStart2",
                table: "freight_settings");

            migrationBuilder.DropColumn(
                name: "FreightRangeStart3",
                table: "freight_settings");
        }
    }
}
