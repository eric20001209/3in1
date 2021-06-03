using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace eCommerce_API_RST.Migrations
{
    public partial class addTableDpsOutput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DpsOutput",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(nullable: false),
                    InvoiceNumber = table.Column<int>(nullable: false),
                    Valid = table.Column<string>(nullable: true),
                    AmountSettlement = table.Column<string>(nullable: true),
                    AuthCode = table.Column<string>(nullable: true),
                    CardName = table.Column<string>(nullable: true),
                    CardNumber = table.Column<string>(nullable: true),
                    DateExpiry = table.Column<string>(nullable: true),
                    DpsTxnRef = table.Column<string>(nullable: true),
                    Success = table.Column<string>(nullable: true),
                    ResponseText = table.Column<string>(nullable: true),
                    DpsBillingId = table.Column<string>(nullable: true),
                    CardHolderName = table.Column<string>(nullable: true),
                    CurrencySettlement = table.Column<string>(nullable: true),
                    PaymentMethod = table.Column<string>(nullable: true),
                    TxnData1 = table.Column<string>(nullable: true),
                    TxnData2 = table.Column<string>(nullable: true),
                    TxnData3 = table.Column<string>(nullable: true),
                    TxnType = table.Column<string>(nullable: true),
                    CurrencyInput = table.Column<string>(nullable: true),
                    MerchantReference = table.Column<string>(nullable: true),
                    ClientInfo = table.Column<string>(nullable: true),
                    TxnId = table.Column<string>(nullable: true),
                    EmailAddress = table.Column<string>(nullable: true),
                    BillingId = table.Column<string>(nullable: true),
                    TxnMac = table.Column<string>(nullable: true),
                    CardNumber2 = table.Column<string>(nullable: true),
                    Cvc2ResultCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DpsOutput", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DpsOutput");
        }
    }
}
