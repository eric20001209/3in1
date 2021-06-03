using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace eCommerce_API_RST.Migrations.Freight
{
    public partial class addfrieighRange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "freight_settings",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    active = table.Column<bool>(nullable: false, defaultValueSql: "((1))"),
                    region = table.Column<string>(maxLength: 250, nullable: true),
                    freight = table.Column<decimal>(type: "money", nullable: false),
                    freeshipping_active_amount = table.Column<decimal>(type: "money", nullable: false),
                    FreightRangeStart1 = table.Column<decimal>(nullable: false),
                    FreightRangeStart2 = table.Column<decimal>(nullable: false),
                    FreightRangeStart3 = table.Column<decimal>(nullable: false),
                    FreightRangeEnd1 = table.Column<decimal>(nullable: false),
                    FreightRangeEnd2 = table.Column<decimal>(nullable: false),
                    FreightRangeEnd3 = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_freight_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    cat = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    name = table.Column<string>(unicode: false, maxLength: 64, nullable: true),
                    value = table.Column<string>(unicode: false, maxLength: 1024, nullable: true),
                    description = table.Column<string>(unicode: false, maxLength: 1024, nullable: true),
                    hidden = table.Column<bool>(nullable: false, defaultValueSql: "(0)"),
                    bool_value = table.Column<bool>(nullable: false, defaultValueSql: "(0)"),
                    access = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_settings_cat",
                table: "settings",
                column: "cat");

            migrationBuilder.CreateIndex(
                name: "IDX_settings_hidden",
                table: "settings",
                column: "hidden");

            migrationBuilder.CreateIndex(
                name: "IDX_settings_name",
                table: "settings",
                column: "name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "freight_settings");

            migrationBuilder.DropTable(
                name: "settings");
        }
    }
}
