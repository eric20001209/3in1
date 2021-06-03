using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace eCommerce_API_RST.Migrations
{
    public partial class addSpecials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<string>(
            //    name: "po_number",
            //    table: "orders",
            //    maxLength: 50,
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldUnicode: false,
            //    oldMaxLength: 50,
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "note",
            //    table: "order_item",
            //    maxLength: 255,
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldUnicode: false,
            //    oldMaxLength: 255,
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<int>(
            //    name: "outer_pack",
            //    table: "code_relations",
            //    nullable: true,
            //    oldClrType: typeof(double),
            //    oldNullable: true);

            //migrationBuilder.CreateTable(
            //    name: "specials",
            //    columns: table => new
            //    {
            //        code = table.Column<int>(nullable: false)
            //            .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
            //        price = table.Column<decimal>(type: "money", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_specials", x => x.code);
            //    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "specials");

            migrationBuilder.AlterColumn<string>(
                name: "po_number",
                table: "orders",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "note",
                table: "order_item",
                unicode: false,
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "outer_pack",
                table: "code_relations",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
