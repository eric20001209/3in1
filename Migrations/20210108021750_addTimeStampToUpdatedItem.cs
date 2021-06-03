using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace eCommerce_API_RST.Migrations
{
    public partial class addTimeStampToUpdatedItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "ecom_banner",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(nullable: false)
            //            .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
            //        name = table.Column<string>(nullable: true),
            //        pic_url = table.Column<string>(nullable: true),
            //        href_url = table.Column<string>(nullable: true),
            //        seq = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ecom_banner", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ecom_setting",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(nullable: false)
            //            .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
            //        name = table.Column<string>(nullable: true),
            //        value = table.Column<string>(nullable: true),
            //        active = table.Column<bool>(nullable: false, defaultValueSql: "(1)")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ecom_setting", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ecom_top_menu",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(nullable: false)
            //            .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
            //        description = table.Column<string>(nullable: true),
            //        url = table.Column<string>(nullable: true),
            //        seq = table.Column<int>(nullable: false),
            //        active = table.Column<bool>(nullable: false, defaultValueSql: "(1)")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ecom_top_menu", x => x.id);
            //    });


            migrationBuilder.AddColumn<byte[]>(
                    name: "time_stamp",
                    table: "updated_item",
                    type: "timestamp",
                    nullable : true
                    );

            //migrationBuilder.CreateTable(
            //    name: "updated_item",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(nullable: false)
            //            .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
            //        branch_id = table.Column<int>(nullable: false),
            //        item_code = table.Column<int>(nullable: false),
            //        date_updated = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
            //        time_stamp = table.Column<byte[]>(type: "timestamp", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_updated_item", x => x.id);
            //    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ecom_banner");

            migrationBuilder.DropTable(
                name: "ecom_setting");

            migrationBuilder.DropTable(
                name: "ecom_top_menu");

            migrationBuilder.DropTable(
                name: "updated_item");
        }
    }
}
