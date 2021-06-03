using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using eCommerce_API_RST_Multi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace FarroAPI.Entities
{
    public partial class farroContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly HostDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IDbConnection DbConnection { get; set; }
        private int HostId { get; set; }
        public farroContext()
        {

        }

        public farroContext(DbContextOptions<farroContext> options, IConfiguration configuration, HostDbContext context,
                        IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
            try
            {
                var hostId = _httpContextAccessor.HttpContext.GetRouteValue("hostId").ToString();
                HostId = int.Parse(hostId);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

        }

        public virtual DbSet<Branch> Branch { get; set; }
        public virtual DbSet<Card> Card { get; set; }
        public virtual DbSet<CodeRelations> CodeRelations { get; set; }
        public virtual DbSet<Enum> Enum { get; set; }
        public virtual DbSet<Invoice> Invoice { get; set; }
        public virtual DbSet<OrderItem> OrderItem { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<Sales> Sales { get; set; }
        public virtual DbSet<TranDetail> TranDetail { get; set; }
        public virtual DbSet<TranInvoice> TranInvoice { get; set; }
        public virtual DbSet<Trans> Trans { get; set; }
        public virtual DbSet<Budget> Budget { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var host = _context.HostTenants.Where(c => c.Id == HostId).FirstOrDefault();
            if (host != null)
            {
                var connectionString = host.Connstr;
                DbConnection = new SqlConnection(this._configuration.GetConnectionString(connectionString));
                //optionsBuilder.UseSqlServer(DbConnection.ToString());
                optionsBuilder.UseSqlServer(connectionString);
            }
            else
                DbConnection = new SqlConnection(this._configuration.GetConnectionString("rst374_cloud12Context"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.Property(e => e.BranchId).HasColumnName("Branch_Id");

                entity.Property(e => e.Amount)
                    .HasColumnName("Amount")
                    .HasColumnType("money");

                entity.Property(e => e.Cat)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.SCat)
                    .IsRequired()
                    .HasColumnName("S_Cat")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Branch>(entity =>
            {
                entity.ToTable("branch");

                entity.HasIndex(e => e.Id)
                    .HasName("IDX_branch_id");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Activated)
                    .IsRequired()
                    .HasColumnName("activated")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Address1)
                    .HasColumnName("address1")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Address2)
                    .HasColumnName("address2")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Address3)
                    .HasColumnName("address3")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BranchFooter)
                    .HasColumnName("branch_footer")
                    .HasColumnType("text");

                entity.Property(e => e.BranchHeader)
                    .HasColumnName("branch_header")
                    .HasColumnType("text");

                entity.Property(e => e.BranchPosRecieptFooter)
                    .HasColumnName("branch_pos_reciept_footer")
                    .HasMaxLength(500);

                entity.Property(e => e.BranchPosRecieptHeader)
                    .HasColumnName("branch_pos_reciept_header")
                    .HasMaxLength(500);

                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Country)
                    .HasColumnName("country")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Fax)
                    .HasColumnName("fax")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Postal1)
                    .HasColumnName("postal1")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Postal2)
                    .HasColumnName("postal2")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Postal3)
                    .HasColumnName("postal3")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Seq)
                    .HasColumnName("seq")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.SyncStock).HasColumnName("sync_stock");

                entity.Property(e => e.TaxNum)
                    .HasColumnName("tax_num")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Card>(entity =>
            {
                entity.ToTable("card");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AcceptMassEmail).HasColumnName("accept_mass_email");

                entity.Property(e => e.AccessAdminZone).HasColumnName("access_admin_zone");

                entity.Property(e => e.AccessCashdraw).HasColumnName("access_cashdraw");

                entity.Property(e => e.AccessDatabase).HasColumnName("access_database");

                entity.Property(e => e.AccessDeleteItem).HasColumnName("access_delete_item");

                entity.Property(e => e.AccessDiscount).HasColumnName("access_discount");

                entity.Property(e => e.AccessLevel)
                    .HasColumnName("access_level")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.AccessProduct).HasColumnName("access_product");

                entity.Property(e => e.AccessRefund).HasColumnName("access_refund");

                entity.Property(e => e.AccessReport).HasColumnName("access_report");

                entity.Property(e => e.AccessSetting).HasColumnName("access_setting");

                entity.Property(e => e.AccessStock).HasColumnName("access_stock");

                entity.Property(e => e.AccessVipPayment).HasColumnName("access_vip_payment");

                entity.Property(e => e.AccessXTotal).HasColumnName("access_x_total");

                entity.Property(e => e.Address1)
                    .HasColumnName("address1")
                    .HasMaxLength(250);

                entity.Property(e => e.Address1B)
                    .HasColumnName("address1B")
                    .HasMaxLength(250);

                entity.Property(e => e.Address2)
                    .HasColumnName("address2")
                    .HasMaxLength(250);

                entity.Property(e => e.Address2B)
                    .HasColumnName("address2B")
                    .HasMaxLength(250);

                entity.Property(e => e.Address3)
                    .HasColumnName("address3")
                    .HasMaxLength(250);

                entity.Property(e => e.ApDdi)
                    .HasColumnName("ap_ddi")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApEmail)
                    .HasColumnName("ap_email")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApMobile)
                    .HasColumnName("ap_mobile")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApName)
                    .HasColumnName("ap_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Approved)
                    .HasColumnName("approved")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Balance)
                    .HasColumnName("balance")
                    .HasColumnType("money");

                entity.Property(e => e.Barcode)
                    .HasColumnName("barcode")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BranchCardId).HasColumnName("branch_card_id");

                entity.Property(e => e.BuyOnline).HasColumnName("buy_online");

                entity.Property(e => e.CatAccess)
                    .HasColumnName("cat_access")
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CatAccessGroup).HasColumnName("cat_access_group");

                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasMaxLength(250);

                entity.Property(e => e.CityB)
                    .HasColumnName("cityB")
                    .HasMaxLength(250);

                entity.Property(e => e.Company)
                    .HasColumnName("company")
                    .HasMaxLength(250);

                entity.Property(e => e.CompanyB)
                    .HasColumnName("companyB")
                    .HasMaxLength(250);

                entity.Property(e => e.Contact)
                    .HasColumnName("contact")
                    .HasMaxLength(250)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CorpNumber)
                    .HasColumnName("corp_number")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Country)
                    .HasColumnName("country")
                    .HasMaxLength(250)
                    .HasDefaultValueSql("('New Zealand')");

                entity.Property(e => e.CountryB)
                    .HasColumnName("countryB")
                    .HasMaxLength(250)
                    .HasDefaultValueSql("('New Zealand')");

                entity.Property(e => e.CreditLimit)
                    .HasColumnName("credit_limit")
                    .HasColumnType("money");

                entity.Property(e => e.CreditTerm)
                    .HasColumnName("credit_term")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CurrencyForPurchase)
                    .HasColumnName("currency_for_purchase")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CustomerAccessLevel)
                    .HasColumnName("customer_access_level")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.DealerLevel)
                    .HasColumnName("dealer_level")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Directory)
                    .HasColumnName("directory")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(250);

                entity.Property(e => e.ExpiredDate)
                    .HasColumnName("expired_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Fax)
                    .HasColumnName("fax")
                    .HasMaxLength(250);

                entity.Property(e => e.GstRate)
                    .HasColumnName("gst_rate")
                    .HasDefaultValueSql("((0.15))");

                entity.Property(e => e.HasExpired).HasColumnName("has_expired");

                entity.Property(e => e.IsBranch).HasColumnName("is_branch");

                entity.Property(e => e.IsQualified).HasColumnName("is_qualified");

                entity.Property(e => e.Language)
                    .HasColumnName("language")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastBranchId).HasColumnName("last_branch_id");

                entity.Property(e => e.LastPostTime)
                    .HasColumnName("last_post_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.M1)
                    .HasColumnName("m1")
                    .HasColumnType("money");

                entity.Property(e => e.M10)
                    .HasColumnName("m10")
                    .HasColumnType("money");

                entity.Property(e => e.M11)
                    .HasColumnName("m11")
                    .HasColumnType("money");

                entity.Property(e => e.M12)
                    .HasColumnName("m12")
                    .HasColumnType("money");

                entity.Property(e => e.M2)
                    .HasColumnName("m2")
                    .HasColumnType("money");

                entity.Property(e => e.M3)
                    .HasColumnName("m3")
                    .HasColumnType("money");

                entity.Property(e => e.M4)
                    .HasColumnName("m4")
                    .HasColumnType("money");

                entity.Property(e => e.M5)
                    .HasColumnName("m5")
                    .HasColumnType("money");

                entity.Property(e => e.M6)
                    .HasColumnName("m6")
                    .HasColumnType("money");

                entity.Property(e => e.M7)
                    .HasColumnName("m7")
                    .HasColumnType("money");

                entity.Property(e => e.M8)
                    .HasColumnName("m8")
                    .HasColumnType("money");

                entity.Property(e => e.M9)
                    .HasColumnName("m9")
                    .HasColumnType("money");

                entity.Property(e => e.MDiscountRate).HasColumnName("m_discount_rate");

                entity.Property(e => e.MType).HasColumnName("m_type");

                entity.Property(e => e.MainCardId).HasColumnName("main_card_id");

                entity.Property(e => e.MemberGroup)
                    .HasColumnName("member_group")
                    .HasDefaultValueSql("((9))");

                entity.Property(e => e.Mobile)
                    .HasColumnName("mobile")
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(250);

                entity.Property(e => e.NameB)
                    .HasColumnName("nameB")
                    .HasMaxLength(250);

                entity.Property(e => e.NoSysQuote).HasColumnName("no_sys_quote");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(250);

                entity.Property(e => e.OurBranch)
                    .HasColumnName("our_branch")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PersonalId).HasColumnName("personal_id");

                entity.Property(e => e.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(250);

                entity.Property(e => e.PmDdi)
                    .HasColumnName("pm_ddi")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PmEmail)
                    .HasColumnName("pm_email")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PmMobile)
                    .HasColumnName("pm_mobile")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.Postal1)
                    .HasColumnName("postal1")
                    .HasMaxLength(250);

                entity.Property(e => e.Postal2)
                    .HasColumnName("postal2")
                    .HasMaxLength(250);

                entity.Property(e => e.Postal3)
                    .HasColumnName("postal3")
                    .HasMaxLength(250);

                entity.Property(e => e.PriceLevel)
                    .HasColumnName("price_level")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Productline)
                    .HasColumnName("productline")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PurchaseAverage)
                    .HasColumnName("purchase_average")
                    .HasColumnType("money");

                entity.Property(e => e.PurchaseNza)
                    .HasColumnName("purchase_nza")
                    .HasColumnType("money");

                entity.Property(e => e.RegisterDate)
                    .HasColumnName("register_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Registered)
                    .IsRequired()
                    .HasColumnName("registered")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Sales).HasColumnName("sales");

                entity.Property(e => e.ShippingFee)
                    .HasColumnName("shipping_fee")
                    .HasColumnType("money")
                    .HasDefaultValueSql("((10))");

                entity.Property(e => e.ShortName)
                    .HasColumnName("short_name")
                    .HasMaxLength(150);

                entity.Property(e => e.SmDdi)
                    .HasColumnName("sm_ddi")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SmEmail)
                    .HasColumnName("sm_email")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SmMobile)
                    .HasColumnName("sm_mobile")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SmName)
                    .HasColumnName("sm_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                //entity.Property(e => e.StockReceiveNoticeEmail)
                //    .HasColumnName("stock_receive_notice_email")
                //    .HasMaxLength(250)
                //    .IsUnicode(false);

                entity.Property(e => e.StopOrder).HasColumnName("stop_order");

                entity.Property(e => e.StopOrderReason)
                    .HasColumnName("stop_order_reason")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                //entity.Property(e => e.SupplierGstNumber)
                //    .HasColumnName("supplier_gst_number")
                //    .HasMaxLength(50)
                //    .IsUnicode(false);

                entity.Property(e => e.TechEmail)
                    .HasColumnName("tech_email")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalOnlineOrderPoint)
                    .HasColumnName("total_online_order_point")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.TotalPosts).HasColumnName("total_posts");

                entity.Property(e => e.TradingName)
                    .HasColumnName("trading_name")
                    .HasMaxLength(250);

                entity.Property(e => e.TransTotal)
                    .HasColumnName("trans_total")
                    .HasColumnType("money");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Web)
                    .HasColumnName("web")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.WorkingOn)
                    .HasColumnName("working_on")
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<CodeRelations>(entity =>
            {
                entity.ToTable("code_relations");

                entity.HasIndex(e => e.Cat)
                    .HasName("IDX_code_relations_cat");

                entity.HasIndex(e => e.Clearance)
                    .HasName("IDX_code_relations_clearance");

                entity.HasIndex(e => e.Code)
                    .HasName("IDX_code_relations_code");

                entity.HasIndex(e => e.Id)
                    .HasName("IDX_code_relations_id");

                entity.HasIndex(e => e.SCat)
                    .HasName("IDX_code_relations_scat");

                entity.HasIndex(e => e.SsCat)
                    .HasName("IDX_code_relations_sscat");

                entity.HasIndex(e => e.SupplierCode)
                    .HasName("IDX_code_relations_spl_code");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AllocatedStock).HasColumnName("allocated_stock");

                entity.Property(e => e.AverageCost)
                    .HasColumnName("average_cost")
                    .HasColumnType("money");

                entity.Property(e => e.AvoidPoint).HasColumnName("avoid_point");

                entity.Property(e => e.Barcode)
                    .HasColumnName("barcode")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BestBefore)
                    .HasColumnName("best_before")
                    .HasMaxLength(50);

                entity.Property(e => e.BomId).HasColumnName("bom_id");

                entity.Property(e => e.BoxedQty)
                    .HasColumnName("boxed_qty")
                    .HasMaxLength(50);

                entity.Property(e => e.Brand)
                    .HasColumnName("brand")
                    .HasMaxLength(50);

                entity.Property(e => e.Cat)
                    .HasColumnName("cat")
                    .HasMaxLength(50);

                entity.Property(e => e.CentralWarehouse).HasColumnName("central_warehouse");

                entity.Property(e => e.Clearance).HasColumnName("clearance");

                entity.Property(e => e.Code).HasColumnName("code");

                entity.Property(e => e.ComingSoon)
                    .HasColumnName("coming_soon")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.CommissionRate).HasColumnName("commission_rate");

                entity.Property(e => e.CostAccount).HasColumnName("cost_account");

                entity.Property(e => e.CostofsalesAccount)
                    .HasColumnName("costofsales_account")
                    .HasDefaultValueSql("((5111))");

                entity.Property(e => e.CountryOfOrigin)
                    .HasColumnName("country_of_origin")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Currency)
                    .HasColumnName("currency")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.DateRange)
                    .HasColumnName("date_range")
                    .HasColumnType("datetime");

                entity.Property(e => e.Disappeared).HasColumnName("disappeared");

                entity.Property(e => e.DoNotRounddown).HasColumnName("do_not_rounddown");

                entity.Property(e => e.ExchangeRate).HasColumnName("exchange_rate");

                entity.Property(e => e.ExpireDate)
                    .HasColumnName("expire_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ForeignSupplierPrice)
                    .HasColumnName("foreign_supplier_price")
                    .HasColumnType("money");

                entity.Property(e => e.HasScale).HasColumnName("has_scale");

                entity.Property(e => e.Hidden).HasColumnName("hidden");

                entity.Property(e => e.HoldPurchase).HasColumnName("hold_purchase");

                entity.Property(e => e.Hot)
                    .IsRequired()
                    .HasColumnName("hot")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Inactive)
                    .HasColumnName("inactive")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.IncomeAccount).HasColumnName("income_account");

                entity.Property(e => e.InnerPack)
                    .HasColumnName("inner_pack")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.InventoryAccount).HasColumnName("inventory_account");

                entity.Property(e => e.IsBarcodeprice).HasColumnName("is_barcodeprice");

                entity.Property(e => e.IsIdCheck).HasColumnName("is_id_check");

                entity.Property(e => e.IsMemberOnly).HasColumnName("is_member_only");

                entity.Property(e => e.IsService).HasColumnName("is_service");

                entity.Property(e => e.IsSpecial).HasColumnName("is_special");

                entity.Property(e => e.IsVoidDiscount).HasColumnName("is_void_discount");

                entity.Property(e => e.IsWebsiteItem).HasColumnName("is_website_item");

                entity.Property(e => e.KitId).HasColumnName("kit_id");

                entity.Property(e => e.LevelPrice0)
                    .HasColumnName("level_price0")
                    .HasColumnType("money");

                entity.Property(e => e.LevelPrice1)
                    .HasColumnName("level_price1")
                    .HasColumnType("money");

                entity.Property(e => e.LevelPrice2)
                    .HasColumnName("level_price2")
                    .HasColumnType("money");

                entity.Property(e => e.LevelPrice3)
                    .HasColumnName("level_price3")
                    .HasColumnType("money");

                entity.Property(e => e.LevelPrice4)
                    .HasColumnName("level_price4")
                    .HasColumnType("money");

                entity.Property(e => e.LevelPrice5)
                    .HasColumnName("level_price5")
                    .HasColumnType("money");

                entity.Property(e => e.LevelPrice6)
                    .HasColumnName("level_price6")
                    .HasColumnType("money");

                entity.Property(e => e.LevelPrice7)
                    .HasColumnName("level_price7")
                    .HasColumnType("money");

                entity.Property(e => e.LevelPrice8)
                    .HasColumnName("level_price8")
                    .HasColumnType("money");

                entity.Property(e => e.LevelPrice9)
                    .HasColumnName("level_price9")
                    .HasColumnType("money");

                entity.Property(e => e.LevelRate1)
                    .HasColumnName("level_rate1")
                    .HasDefaultValueSql("((100))");

                entity.Property(e => e.LevelRate2)
                    .HasColumnName("level_rate2")
                    .HasDefaultValueSql("((95))");

                entity.Property(e => e.LevelRate3)
                    .HasColumnName("level_rate3")
                    .HasDefaultValueSql("((90))");

                entity.Property(e => e.LevelRate4)
                    .HasColumnName("level_rate4")
                    .HasDefaultValueSql("((85))");

                entity.Property(e => e.LevelRate5)
                    .HasColumnName("level_rate5")
                    .HasDefaultValueSql("((80))");

                entity.Property(e => e.LevelRate6)
                    .HasColumnName("level_rate6")
                    .HasDefaultValueSql("((78))");

                entity.Property(e => e.LevelRate7)
                    .HasColumnName("level_rate7")
                    .HasDefaultValueSql("((75))");

                entity.Property(e => e.LevelRate8)
                    .HasColumnName("level_rate8")
                    .HasDefaultValueSql("((73))");

                entity.Property(e => e.LevelRate9)
                    .HasColumnName("level_rate9")
                    .HasDefaultValueSql("((70))");

                entity.Property(e => e.Line1Font)
                    .HasColumnName("line1_font")
                    .HasDefaultValueSql("((9))");

                entity.Property(e => e.Line2Font)
                    .HasColumnName("line2_font")
                    .HasDefaultValueSql("((9))");

                entity.Property(e => e.LowStock).HasColumnName("low_stock");

                entity.Property(e => e.ManualCostFrd)
                    .HasColumnName("manual_cost_frd")
                    .HasColumnType("money");

                entity.Property(e => e.ManualCostNzd)
                    .HasColumnName("manual_cost_nzd")
                    .HasColumnType("money");

                entity.Property(e => e.ManualExchangeRate)
                    .HasColumnName("manual_exchange_rate")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Moq)
                    .HasColumnName("moq")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255);

                entity.Property(e => e.NameCn)
                    .HasColumnName("name_cn")
                    .HasMaxLength(250);

                entity.Property(e => e.NewItem).HasColumnName("new_item");

                entity.Property(e => e.NewItemDate)
                    .HasColumnName("new_item_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.NoDiscount).HasColumnName("no_discount");

                entity.Property(e => e.NormalPrice).HasColumnName("normal_price");

                entity.Property(e => e.NzdFreight)
                    .HasColumnName("nzd_freight")
                    .HasColumnType("money");

                entity.Property(e => e.OuterPackBarcode)
                    .HasColumnName("outer_pack_barcode")
                    .HasMaxLength(99)
                    .IsUnicode(false);

                entity.Property(e => e.PackageBarcode1)
                    .HasColumnName("package_barcode1")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PackageBarcode2)
                    .HasColumnName("package_barcode2")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PackageBarcode3)
                    .HasColumnName("package_barcode3")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PackagePrice1).HasColumnName("package_price1");

                entity.Property(e => e.PackagePrice2).HasColumnName("package_price2");

                entity.Property(e => e.PackagePrice3).HasColumnName("package_price3");

                entity.Property(e => e.PackageQty1).HasColumnName("package_qty1");

                entity.Property(e => e.PackageQty2).HasColumnName("package_qty2");

                entity.Property(e => e.PackageQty3).HasColumnName("package_qty3");

                entity.Property(e => e.PickDate).HasColumnName("pick_date");

                entity.Property(e => e.Popular)
                    .IsRequired()
                    .HasColumnName("popular")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Price1)
                    .HasColumnName("price1")
                    .HasColumnType("money");

                entity.Property(e => e.Price2)
                    .HasColumnName("price2")
                    .HasColumnType("money");

                entity.Property(e => e.Price3)
                    .HasColumnName("price3")
                    .HasColumnType("money");

                entity.Property(e => e.Price4)
                    .HasColumnName("price4")
                    .HasColumnType("money");

                entity.Property(e => e.Price5)
                    .HasColumnName("price5")
                    .HasColumnType("money");

                entity.Property(e => e.Price6)
                    .HasColumnName("price6")
                    .HasColumnType("money");

                entity.Property(e => e.Price7)
                    .HasColumnName("price7")
                    .HasColumnType("money");

                entity.Property(e => e.Price8)
                    .HasColumnName("price8")
                    .HasColumnType("money");

                entity.Property(e => e.Price9)
                    .HasColumnName("price9")
                    .HasColumnType("money");

                entity.Property(e => e.PriceSystem)
                    .HasColumnName("price_system")
                    .HasColumnType("money");

                entity.Property(e => e.PrintFormatCode).HasColumnName("print_format_code");

                entity.Property(e => e.ProductCode)
                    .HasColumnName("product_code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PromoId)
                    .HasColumnName("promo_id")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Promotion)
                    .HasColumnName("promotion")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.QposQtyBreak).HasColumnName("qpos_qty_break");

                entity.Property(e => e.QtyBreak1)
                    .HasColumnName("qty_break1")
                    .HasDefaultValueSql("((5))");

                entity.Property(e => e.QtyBreak2)
                    .HasColumnName("qty_break2")
                    .HasDefaultValueSql("((10))");

                entity.Property(e => e.QtyBreak3)
                    .HasColumnName("qty_break3")
                    .HasDefaultValueSql("((20))");

                entity.Property(e => e.QtyBreak4)
                    .HasColumnName("qty_break4")
                    .HasDefaultValueSql("((50))");

                entity.Property(e => e.QtyBreak5).HasColumnName("qty_break5");

                entity.Property(e => e.QtyBreak6).HasColumnName("qty_break6");

                entity.Property(e => e.QtyBreak7).HasColumnName("qty_break7");

                entity.Property(e => e.QtyBreak8).HasColumnName("qty_break8");

                entity.Property(e => e.QtyBreak9).HasColumnName("qty_break9");

                entity.Property(e => e.QtyBreakDiscount1).HasColumnName("qty_break_discount1");

                entity.Property(e => e.QtyBreakDiscount2).HasColumnName("qty_break_discount2");

                entity.Property(e => e.QtyBreakDiscount3).HasColumnName("qty_break_discount3");

                entity.Property(e => e.QtyBreakDiscount4).HasColumnName("qty_break_discount4");

                entity.Property(e => e.QtyBreakDiscount5).HasColumnName("qty_break_discount5");

                entity.Property(e => e.QtyBreakDiscount6).HasColumnName("qty_break_discount6");

                entity.Property(e => e.QtyBreakDiscount7).HasColumnName("qty_break_discount7");

                entity.Property(e => e.QtyBreakDiscount8).HasColumnName("qty_break_discount8");

                entity.Property(e => e.QtyBreakDiscount9).HasColumnName("qty_break_discount9");

                entity.Property(e => e.QtyBreakPrice1)
                    .HasColumnName("qty_break_price1")
                    .HasColumnType("money");

                entity.Property(e => e.QtyBreakPrice10)
                    .HasColumnName("qty_break_price10")
                    .HasColumnType("money");

                entity.Property(e => e.QtyBreakPrice2)
                    .HasColumnName("qty_break_price2")
                    .HasColumnType("money");

                entity.Property(e => e.QtyBreakPrice3)
                    .HasColumnName("qty_break_price3")
                    .HasColumnType("money");

                entity.Property(e => e.QtyBreakPrice4)
                    .HasColumnName("qty_break_price4")
                    .HasColumnType("money");

                entity.Property(e => e.QtyBreakPrice5)
                    .HasColumnName("qty_break_price5")
                    .HasColumnType("money");

                entity.Property(e => e.QtyBreakPrice6)
                    .HasColumnName("qty_break_price6")
                    .HasColumnType("money");

                entity.Property(e => e.QtyBreakPrice7)
                    .HasColumnName("qty_break_price7")
                    .HasColumnType("money");

                entity.Property(e => e.QtyBreakPrice8)
                    .HasColumnName("qty_break_price8")
                    .HasColumnType("money");

                entity.Property(e => e.QtyBreakPrice9)
                    .HasColumnName("qty_break_price9")
                    .HasColumnType("money");

                entity.Property(e => e.Rate)
                    .HasColumnName("rate")
                    .HasDefaultValueSql("((1.1))");

                entity.Property(e => e.RealStock).HasColumnName("real_stock");

                entity.Property(e => e.RedeemPoint).HasColumnName("redeem_point");

                entity.Property(e => e.RefCode)
                    .HasColumnName("ref_code")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Rrp)
                    .HasColumnName("rrp")
                    .HasColumnType("money");

                entity.Property(e => e.SCat)
                    .HasColumnName("s_cat")
                    .HasMaxLength(50);

                entity.Property(e => e.ScaleDescriptionLine1)
                    .HasColumnName("scale_description_line1")
                    .HasMaxLength(50);

                entity.Property(e => e.ScaleDescriptionLine2)
                    .HasColumnName("scale_description_line2")
                    .HasMaxLength(50);

                entity.Property(e => e.SellBy)
                    .HasColumnName("sell_by")
                    .HasMaxLength(50);

                entity.Property(e => e.Skip).HasColumnName("skip");

                entity.Property(e => e.SkuCode)
                    .HasColumnName("sku_code")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Sor).HasColumnName("sor");

                entity.Property(e => e.SpecialCost)
                    .HasColumnName("special_cost")
                    .HasColumnType("money");

                entity.Property(e => e.SpecialCostEndDate)
                    .HasColumnName("special_cost_end_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.SpecialCostStartDate)
                    .HasColumnName("special_cost_start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.SpecialPrice).HasColumnName("special_price");

                entity.Property(e => e.SpecialPriceEndDate)
                    .HasColumnName("special_price_end_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SpecialPriceStartDate)
                    .HasColumnName("special_price_start_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SsCat)
                    .HasColumnName("ss_cat")
                    .HasMaxLength(50);

                entity.Property(e => e.StockLocation)
                    .HasColumnName("stock_location")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Supplier)
                    .HasColumnName("supplier")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SupplierCode)
                    .HasColumnName("supplier_code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SupplierPrice)
                    .HasColumnName("supplier_price")
                    .HasColumnType("money")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Tareweight)
                    .HasColumnName("tareweight")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.TaxCode)
                    .HasColumnName("tax_code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxRate)
                    .HasColumnName("tax_rate")
                    .HasDefaultValueSql("((0.15))");

                entity.Property(e => e.Unit)
                    .HasColumnName("unit")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.UsedBy)
                    .HasColumnName("used_by")
                    .HasMaxLength(50);

                entity.Property(e => e.Weight)
                    .HasColumnName("weight")
                    .HasDefaultValueSql("((10))");
            });

            modelBuilder.Entity<Enum>(entity =>
            {
                entity.HasKey(e => new { e.Class, e.Id });

                entity.ToTable("enum");

                entity.HasIndex(e => e.Class)
                    .HasName("IDX_enum_class");

                entity.HasIndex(e => e.Id)
                    .HasName("IDX_enum_class_id");

                entity.HasIndex(e => e.Name)
                    .HasName("IDX_enum_name");

                entity.Property(e => e.Class)
                    .HasColumnName("class")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoice");

                entity.HasIndex(e => e.Branch)
                    .HasName("IDX_invoice_branch");

                entity.HasIndex(e => e.CardId)
                    .HasName("IDX_invoice_card_id");

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.InvoiceNumber)
                    .HasName("IDX_invoice_number")
                    .IsUnique();

                entity.HasIndex(e => e.Type)
                    .HasName("IDX_invoice_type");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address1)
                    .HasColumnName("address1")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Address2)
                    .HasColumnName("address2")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Address3)
                    .HasColumnName("address3")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Agent).HasColumnName("agent");

                entity.Property(e => e.AmountPaid)
                    .HasColumnName("amount_paid")
                    .HasColumnType("money")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Barcode)
                    .HasColumnName("barcode")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Branch)
                    .HasColumnName("branch")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CardId).HasColumnName("card_id");

                entity.Property(e => e.CommitDate)
                    .HasColumnName("commit_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Company)
                    .HasColumnName("company")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CustPonumber)
                    .HasColumnName("cust_ponumber")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerGst)
                    .HasColumnName("customer_gst")
                    .HasDefaultValueSql("((0.15))");

                entity.Property(e => e.DebugInfo)
                    .HasColumnName("debug_info")
                    .HasMaxLength(2048)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryNumber)
                    .HasColumnName("delivery_number")
                    .HasMaxLength(255);

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasColumnName("fax")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Freight)
                    .HasColumnName("freight")
                    .HasColumnType("money")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.GstInclusive).HasColumnName("gst_inclusive");

                entity.Property(e => e.InvoiceNumber).HasColumnName("invoice_number");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.NoIndividualPrice).HasColumnName("no_individual_price");

                entity.Property(e => e.Paid).HasColumnName("paid");

                entity.Property(e => e.PaymentType)
                    .HasColumnName("payment_type")
                    .HasDefaultValueSql("((2))");

                entity.Property(e => e.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PickUpTime)
                    .HasColumnName("pick_up_time")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Postal1)
                    .HasColumnName("postal1")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Postal2)
                    .HasColumnName("postal2")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Postal3)
                    .HasColumnName("postal3")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("money");

                entity.Property(e => e.RecordDate)
                    .HasColumnName("record_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Refunded).HasColumnName("refunded");

                entity.Property(e => e.Sales)
                    .HasColumnName("sales")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SalesNote)
                    .HasColumnName("sales_note")
                    .HasColumnType("ntext");

                entity.Property(e => e.SalesType)
                    .HasColumnName("sales_type")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ShippingMethod)
                    .HasColumnName("shipping_method")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Shipto)
                    .HasColumnName("shipto")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.SpecialShipto).HasColumnName("special_shipto");

                entity.Property(e => e.StationId).HasColumnName("station_id");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.System).HasColumnName("system");

                entity.Property(e => e.Tax)
                    .HasColumnName("tax")
                    .HasColumnType("money");

                entity.Property(e => e.Total)
                    .HasColumnName("total")
                    .HasColumnType("money");

                entity.Property(e => e.TradingName)
                    .HasColumnName("trading_name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TransFailedReason)
                    .HasColumnName("trans_failed_reason")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("((3))");

                entity.Property(e => e.Uploaded).HasColumnName("uploaded");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Kid);

                entity.ToTable("order_item");

                entity.HasIndex(e => e.Code)
                    .HasName("IDX_order_item_code");

                entity.HasIndex(e => e.Id)
                    .HasName("IDX_order_item_id");

                entity.HasIndex(e => e.Kid)
                    .HasName("IDX_order_item_krid");

                entity.HasIndex(e => e.Kit)
                    .HasName("IDX_order_item_kit");

                entity.HasIndex(e => e.SupplierCode)
                    .HasName("IDX_order_item_supplier_code");

                entity.Property(e => e.Kid).HasColumnName("kid");

                entity.Property(e => e.Barcode)
                    .HasColumnName("barcode")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Cat)
                    .HasColumnName("cat")
                    .HasMaxLength(150);

                entity.Property(e => e.Code).HasColumnName("code");

                entity.Property(e => e.CommitPrice)
                    .HasColumnName("commit_price")
                    .HasColumnType("money");

                entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent");

                entity.Property(e => e.Eta)
                    .HasColumnName("eta")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasColumnName("item_name")
                    .HasMaxLength(255);

                entity.Property(e => e.ItemNameCn)
                    .HasColumnName("item_name_cn")
                    .HasMaxLength(150);

                entity.Property(e => e.Kit).HasColumnName("kit");

                entity.Property(e => e.Krid).HasColumnName("krid");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.OrderTotal)
                    .HasColumnName("order_total")
                    .HasColumnType("money");

                entity.Property(e => e.Pack)
                    .HasColumnName("pack")
                    .HasMaxLength(100);

                entity.Property(e => e.Part)
                    .HasColumnName("part")
                    .HasDefaultValueSql("((-1))");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantitySupplied).HasColumnName("quantity_supplied");

                entity.Property(e => e.SCat)
                    .HasColumnName("s_cat")
                    .HasMaxLength(150);

                entity.Property(e => e.SsCat)
                    .HasColumnName("ss_cat")
                    .HasMaxLength(150);

                entity.Property(e => e.StationId).HasColumnName("station_id");

                entity.Property(e => e.Supplier)
                    .IsRequired()
                    .HasColumnName("supplier")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SupplierCode)
                    .IsRequired()
                    .HasColumnName("supplier_code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SupplierPrice)
                    .HasColumnName("supplier_price")
                    .HasColumnType("money");

                entity.Property(e => e.SysSpecial).HasColumnName("sys_special");

                entity.Property(e => e.System).HasColumnName("system");
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.ToTable("orders");

                entity.HasIndex(e => e.CardId)
                    .HasName("IDX_orders_card_id");

                entity.HasIndex(e => e.Id)
                    .HasName("IDX_orders_id")
                    .IsUnique();

                entity.HasIndex(e => e.InvoiceNumber)
                    .HasName("IDX_orders_invoice_number");

                entity.HasIndex(e => e.Sales)
                    .HasName("IDX_orders_sales");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Agent).HasColumnName("agent");

                entity.Property(e => e.Branch)
                    .HasColumnName("branch")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CCardName)
                    .HasColumnName("cCardName")
                    .HasMaxLength(150);

                entity.Property(e => e.CCardNum)
                    .HasColumnName("cCardNum")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.CCardType)
                    .HasColumnName("cCardType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CRefCode)
                    .HasColumnName("cRefCode")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.CResponseTxt)
                    .HasColumnName("cResponseTxt")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.CSuccess)
                    .HasColumnName("cSuccess")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CardId).HasColumnName("card_id");

                entity.Property(e => e.Contact)
                    .HasColumnName("contact")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreditOrderId).HasColumnName("credit_order_id");

                entity.Property(e => e.CustomerGst)
                    .HasColumnName("customer_gst")
                    .HasDefaultValueSql("((0.15))");

                entity.Property(e => e.DateShipped)
                    .HasColumnName("date_shipped")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DealerCustomerName)
                    .HasColumnName("dealer_customer_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DealerDraft).HasColumnName("dealer_draft");

                entity.Property(e => e.DealerTotal)
                    .HasColumnName("dealer_total")
                    .HasColumnType("money");

                entity.Property(e => e.DebugInfo)
                    .HasColumnName("debug_info")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryNumber)
                    .HasColumnName("delivery_number")
                    .HasMaxLength(255);

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.Freight)
                    .HasColumnName("freight")
                    .HasColumnType("money");

                entity.Property(e => e.GstInclusive).HasColumnName("gst_inclusive");

                entity.Property(e => e.InvoiceNumber).HasColumnName("invoice_number");

                entity.Property(e => e.IsWebOrder).HasColumnName("is_web_order");

                entity.Property(e => e.LockedBy).HasColumnName("locked_by");

                entity.Property(e => e.NoIndividualPrice).HasColumnName("no_individual_price");

                entity.Property(e => e.Number).HasColumnName("number");

                entity.Property(e => e.OnlineProcessed).HasColumnName("online_processed");

                entity.Property(e => e.OrderDeleted).HasColumnName("order_deleted");

                entity.Property(e => e.OrderTotal)
                    .HasColumnName("order_total")
                    .HasColumnType("money");

                entity.Property(e => e.Paid).HasColumnName("paid");

                entity.Property(e => e.Part).HasColumnName("part");

                entity.Property(e => e.PaymentType)
                    .HasColumnName("payment_type")
                    .HasDefaultValueSql("((3))");

                entity.Property(e => e.PickUpTime)
                    .HasColumnName("pick_up_time")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PoNumber)
                    .HasColumnName("po_number")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");

                entity.Property(e => e.QuoteTotal)
                    .HasColumnName("quote_total")
                    .HasColumnType("money");

                entity.Property(e => e.RecordDate)
                    .HasColumnName("record_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Sales).HasColumnName("sales");

                entity.Property(e => e.SalesManager).HasColumnName("sales_manager");

                entity.Property(e => e.SalesNote)
                    .HasColumnName("sales_note")
                    .HasColumnType("ntext");

                entity.Property(e => e.ShipAsParts).HasColumnName("ship_as_parts");

                entity.Property(e => e.Shipby).HasColumnName("shipby");

                entity.Property(e => e.ShippingMethod)
                    .HasColumnName("shipping_method")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Shipto)
                    .HasColumnName("shipto")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.SpecialShipto).HasColumnName("special_shipto");

                entity.Property(e => e.StationId).HasColumnName("station_id");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.StatusOrderonly)
                    .HasColumnName("status_orderonly")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.System).HasColumnName("system");

                entity.Property(e => e.Ticket)
                    .HasColumnName("ticket")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TimeLocked)
                    .HasColumnName("time_locked")
                    .HasColumnType("datetime");

                entity.Property(e => e.TotalCharge)
                    .HasColumnName("total_charge")
                    .HasColumnType("money");

                entity.Property(e => e.TotalDiscount)
                    .HasColumnName("total_discount")
                    .HasColumnType("money");

                entity.Property(e => e.TotalSpecial)
                    .HasColumnName("total_special")
                    .HasColumnType("money");

                entity.Property(e => e.TransFailedReason)
                    .HasColumnName("trans_failed_reason")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("((2))");

                entity.Property(e => e.Unchecked)
                    .IsRequired()
                    .HasColumnName("unchecked")
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<Sales>(entity =>
            {
                entity.ToTable("sales");

                entity.HasIndex(e => e.Code)
                    .HasName("IDX_sales_code");

                entity.HasIndex(e => e.InvoiceNumber)
                    .HasName("IDX_sales_invoice_number");

                entity.HasIndex(e => e.Kit)
                    .HasName("IDX_sales_kit");

                entity.HasIndex(e => e.Krid)
                    .HasName("IDX_sales_krid");

                entity.HasIndex(e => e.Part)
                    .HasName("IDX_sales_part");

                entity.HasIndex(e => e.Status)
                    .HasName("IDX_sales_status");

                entity.HasIndex(e => e.System)
                    .HasName("IDX_sales_system");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Cat)
                    .HasColumnName("cat")
                    .HasMaxLength(150);

                entity.Property(e => e.Code).HasColumnName("code");

                entity.Property(e => e.CommitPrice)
                    .HasColumnName("commit_price")
                    .HasColumnType("money");

                entity.Property(e => e.CostofsalesAccount)
                    .HasColumnName("costofsales_account")
                    .HasDefaultValueSql("((5111))");

                entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent");

                entity.Property(e => e.IncomeAccount)
                    .HasColumnName("income_account")
                    .HasDefaultValueSql("((4111))");

                entity.Property(e => e.InventoryAccount)
                    .HasColumnName("inventory_account")
                    .HasDefaultValueSql("((1121))");

                entity.Property(e => e.InvoiceNumber).HasColumnName("invoice_number");

                entity.Property(e => e.Kit).HasColumnName("kit");

                entity.Property(e => e.Krid).HasColumnName("krid");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255);

                entity.Property(e => e.NameCn)
                    .HasColumnName("name_cn")
                    .HasMaxLength(150);

                entity.Property(e => e.NormalPrice)
                    .HasColumnName("normal_price")
                    .HasColumnType("money");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Owner).HasColumnName("owner");

                entity.Property(e => e.PStatus).HasColumnName("p_status");

                entity.Property(e => e.Pack)
                    .HasColumnName("pack")
                    .HasMaxLength(100);

                entity.Property(e => e.Part)
                    .HasColumnName("part")
                    .HasDefaultValueSql("((-1))");

                entity.Property(e => e.PriceOnSpecial).HasColumnName("price_on_special");

                entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.SCat)
                    .HasColumnName("s_cat")
                    .HasMaxLength(150);

                entity.Property(e => e.SalesTotal)
                    .HasColumnName("sales_total")
                    .HasColumnType("money");

                entity.Property(e => e.SerialNumber)
                    .HasColumnName("serial_number")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShipDate)
                    .HasColumnName("ship_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Shipby).HasColumnName("shipby");

                entity.Property(e => e.SsCat)
                    .HasColumnName("ss_cat")
                    .HasMaxLength(150);

                entity.Property(e => e.StationId).HasColumnName("station_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.StockAtSales).HasColumnName("stock_at_sales");

                entity.Property(e => e.Supplier)
                    .HasColumnName("supplier")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SupplierCode)
                    .HasColumnName("supplier_code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SupplierPrice)
                    .HasColumnName("supplier_price")
                    .HasColumnType("money");

                entity.Property(e => e.SysSpecial).HasColumnName("sys_special");

                entity.Property(e => e.System).HasColumnName("system");

                entity.Property(e => e.Ticket)
                    .HasColumnName("ticket")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Used).HasColumnName("used");
            });

            modelBuilder.Entity<TranDetail>(entity =>
            {
                entity.HasKey(e => e.Kid);

                entity.ToTable("tran_detail");

                entity.HasIndex(e => e.CardId)
                    .HasName("IDX_tran_detail_card_id");

                entity.HasIndex(e => e.Id)
                    .HasName("IDX_tran_detail_id");

                entity.HasIndex(e => e.StaffId);

                entity.Property(e => e.Kid).HasColumnName("kid");

                entity.Property(e => e.Bank)
                    .HasColumnName("bank")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Branch)
                    .HasColumnName("branch")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CardId).HasColumnName("card_id");

                entity.Property(e => e.Credit)
                    .HasColumnName("credit")
                    .HasColumnType("money");

                entity.Property(e => e.CreditId).HasColumnName("credit_id");

                entity.Property(e => e.CurrencyLoss)
                    .HasColumnName("currency_loss")
                    .HasColumnType("money");

                entity.Property(e => e.DestBalance)
                    .HasColumnName("dest_balance")
                    .HasColumnType("money");

                entity.Property(e => e.Finance)
                    .HasColumnName("finance")
                    .HasColumnType("money");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.InvoiceNumber)
                    .HasColumnName("invoice_number")
                    .HasMaxLength(4096)
                    .IsUnicode(false);

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(1024);

                entity.Property(e => e.PaidBy)
                    .HasColumnName("paid_by")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");

                entity.Property(e => e.PaymentRef)
                    .HasColumnName("payment_ref")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceBalance)
                    .HasColumnName("source_balance")
                    .HasColumnType("money");

                entity.Property(e => e.StaffId).HasColumnName("staff_id");

                entity.Property(e => e.TransDate)
                    .HasColumnName("trans_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<TranInvoice>(entity =>
            {
                entity.ToTable("tran_invoice");

                entity.HasIndex(e => e.Purchase)
                    .HasName("IDX_tran_invoice_purchase");

                entity.HasIndex(e => e.TranId)
                    .HasName("IDX_tran_invoice_tranid");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AmountApplied)
                    .HasColumnName("amount_applied")
                    .HasColumnType("money");

                entity.Property(e => e.InvoiceNumber).HasColumnName("invoice_number");

                entity.Property(e => e.Purchase)
                    .IsRequired()
                    .HasColumnName("purchase")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.TranId).HasColumnName("tran_id");
            });

            modelBuilder.Entity<Trans>(entity =>
            {
                entity.ToTable("trans");

                entity.HasIndex(e => e.Banked)
                    .HasName("IDX_trans_banked");

                entity.HasIndex(e => e.Branch)
                    .HasName("IDX_trans_branch");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("money");

                entity.Property(e => e.Banked)
                    .IsRequired()
                    .HasColumnName("banked")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.Branch)
                    .HasColumnName("branch")
                    .HasDefaultValueSql("(1)");

                entity.Property(e => e.Dest).HasColumnName("dest");

                entity.Property(e => e.DestAmount)
                    .HasColumnName("dest_amount")
                    .HasColumnType("money");

                entity.Property(e => e.Reconcile).HasColumnName("reconcile");

                entity.Property(e => e.Source).HasColumnName("source");

                entity.Property(e => e.TransBankId).HasColumnName("trans_bank_id");

                entity.Property(e => e.TransDate).HasColumnName("trans_date");
            });
        }
    }
}
