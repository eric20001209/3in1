using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sync.Models;
using eCommerce_API_RST_Multi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using eCommerce_API_RST_Multi.Models.Sync;

namespace Sync.Data
{
	public partial class AppDbContext : DbContext
	{
        private readonly IConfiguration _configuration;
        private readonly HostDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IDbConnection DbConnection { get; set; }
        private int HostId { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration, HostDbContext context,
                        IHttpContextAccessor httpContextAccessor
                    )
            : base(options)
        {

            _context = context;
            _httpContextAccessor = httpContextAccessor;
            var hostId = _httpContextAccessor.HttpContext.GetRouteValue("hostId").ToString();
                // ttpContextAccessor.HttpContext.Request.RouteValues["hostId"];    //this is for .net core 3

            this._configuration = configuration;
            HostId = int.Parse(hostId);
        }

        public AppDbContext()
        {
        }

        public virtual DbSet<Barcode> Barcode { get; set; }
        public virtual DbSet<Branch> Branch { get; set; }
        public virtual DbSet<Button> Button { get; set; }
        public virtual DbSet<ButtonItem> ButtonItem { get; set; }
        public virtual DbSet<Card> Cards { get; set; }
        public virtual DbSet<Catalog> Category { get; set; }
        public virtual DbSet<CodeBranch> CodeBranch { get; set; }
        public virtual DbSet<CodeRelations> CodeRelations { get; set; }
        public virtual DbSet<Models.Enum> Enums { get; set; }
        public virtual DbSet<InvoiceFreight> InvoiceFreight { get; set; }
        public virtual DbSet<Invoice> Invoice { get; set; }
        public virtual DbSet<OrderItem> OrderItem { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductDetails> ProductDetails { get; set; }
        public virtual DbSet<PromotionList> PromotionLists { get; set; }
        public virtual DbSet<PromotionGroup> PromotionGroups { get; set; }
        public virtual DbSet<PromotionBranch> PromotionBranches { get; set; }
        public virtual DbSet<Sales> Sales { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<StockQty> StockQty { get; set; }
        public virtual DbSet<ShippingInfo> ShippingInfo { get; set; }
        public virtual DbSet<StoreSpecial> StoreSpecial { get; set; }
        public virtual DbSet<TranDetail> TranDetails { get; set; }
        public virtual DbSet<UpdatedBranch> UpdatedBranch{ get; set; }
        public virtual DbSet<UpdatedItem> UpdatedItem { get; set; }
        public virtual DbSet<UpdatedButton> UpdatedButton { get; set; }
        public virtual DbSet<UpdatedCard> UpdatedCard { get; set; }
        public virtual DbSet<UpdatedCategory> UpdatedCategory { get; set; }
        public virtual DbSet<UpdatedPromotion> UpdatedPromotion { get; set; }
        public virtual DbSet<WorkTime> WorkTimes { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
   //       if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                var webapp = _context.webApps.FirstOrDefault(c => c.Id == HostId);
                if (webapp != null)
                {
                    var dbid = webapp.DbId;
                    var host = _context.HostTenants.Where(c => c.Id == dbid).FirstOrDefault();
                    if (host != null)
                    {
                        var connectionString = host.Connstr;
                        DbConnection = new SqlConnection(this._configuration.GetConnectionString(connectionString));
                        optionsBuilder.UseSqlServer(connectionString);
                    }
                    else
                    {
                        DbConnection = new SqlConnection(this._configuration.GetConnectionString("rst374_cloud12Context"));
                        optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("rst374_cloud12Context"));
                    }
                }
                else
                {
                    DbConnection = new SqlConnection(this._configuration.GetConnectionString("rst374_cloud12Context"));
                    optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("rst374_cloud12Context"));
                }
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");
            modelBuilder.Entity<Barcode>(entity =>
            {
                entity.ToTable("barcode");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Barcode1)
                    .IsRequired()
                    .HasColumnName("barcode")
                    .HasMaxLength(255);

                entity.Property(e => e.Bcancelled).HasColumnName("bcancelled");

                entity.Property(e => e.BoxQty).HasColumnName("box_qty");

                entity.Property(e => e.CancelledNote)
                    .HasColumnName("cancelled_note")
                    .HasMaxLength(502);

                entity.Property(e => e.CartonBarcode)
                    .HasColumnName("carton_barcode")
                    .HasMaxLength(255);

                entity.Property(e => e.CartonQty).HasColumnName("carton_qty");

                entity.Property(e => e.InvoiceNumber)
                    .HasColumnName("invoice_number")
                    .HasMaxLength(50);

                entity.Property(e => e.ItemCode).HasColumnName("item_code");

                entity.Property(e => e.ItemQty).HasColumnName("item_qty");

                entity.Property(e => e.PackagePrice)
                    .HasColumnName("package_price")
                    .HasColumnType("money");

                entity.Property(e => e.SupplierCode)
                    .HasColumnName("supplier_code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherAmount)
                    .HasColumnName("voucher_amount")
                    .HasColumnType("money")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.VoucherCreated)
                    .HasColumnName("voucher_created")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.ToTable("branch");
                entity.HasIndex(e => e.Id)
                    .HasName("IDX_branch_id");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Activated).HasColumnName("activated");
                entity.Property(e => e.ApiSync).HasColumnName("api_sync");
                entity.Property(e => e.AuthCode).HasColumnName("auth_code");
            });
            modelBuilder.Entity<Button>(entity =>
            {
                entity.ToTable("button");
                entity.HasIndex(e => e.Id)
                    .HasName("IDX_button_id");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.NameEn).HasColumnName("name_en");
                entity.Property(e => e.IsIndivisual).HasColumnName("is_indivisual");
            });
            modelBuilder.Entity<ButtonItem>(entity =>
            {
                entity.ToTable("button_item");
                entity.HasIndex(e => e.Id)
                    .HasName("IDX_button_item_id");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ButtonId).HasColumnName("button_id");
                entity.Property(e => e.Code).HasColumnName("code");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.NameEn).HasColumnName("name_en");
                entity.Property(e => e.Location).HasColumnName("location");
 
            });

            modelBuilder.Entity<Card>(entity =>
            {
                //entity.HasKey(e => e.Id)
                //    .ForSqlServerIsClustered(false);

                entity.ToTable("card");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AcceptMassEmail).HasColumnName("accept_mass_email");
                entity.Property(e => e.MDiscountRate).HasColumnName("m_discount_rate");
                entity.Property(e => e.Points).HasColumnName("points");
                entity.Property(e => e.Language).HasColumnName("language");

                entity.Property(e => e.AccessLevel)
                    .HasColumnName("access_level")
                    .HasDefaultValueSql("((1))");

                //entity.Property(e => e.AccountNumber)
                //    .HasMaxLength(255)
                //    .HasColumnName("account_number");

                entity.Property(e => e.Address1)
                    .HasMaxLength(50)
                    .HasColumnName("address1");

                entity.Property(e => e.Address1B)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("address1B");

                entity.Property(e => e.Address2)
                    .HasMaxLength(50)
                    .HasColumnName("address2");

                entity.Property(e => e.Address2B)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("address2B");

                entity.Property(e => e.Address3)
                    .HasMaxLength(50)
                    .HasColumnName("address3");

                entity.Property(e => e.ApDdi)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ap_ddi");

                entity.Property(e => e.ApEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ap_email");

                entity.Property(e => e.ApMobile)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ap_mobile");

                entity.Property(e => e.ApName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ap_name");

                entity.Property(e => e.Approved)
                    .HasColumnName("approved")
                    .HasDefaultValueSql("((1))");

                //entity.Property(e => e.Area)
                //    .HasMaxLength(50)
                //    .HasColumnName("area");

                entity.Property(e => e.Balance)
                    .HasColumnType("money")
                    .HasColumnName("balance");

                //entity.Property(e => e.BankName)
                //    .HasMaxLength(255)
                //    .HasColumnName("bank_name");

                entity.Property(e => e.Barcode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("barcode");

                entity.Property(e => e.BranchCardId).HasColumnName("branch_card_id");

                entity.Property(e => e.BuyOnline).HasColumnName("buy_online");

                entity.Property(e => e.CatAccess)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("cat_access")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CatAccessGroup).HasColumnName("cat_access_group");

                entity.Property(e => e.City)
                    .HasMaxLength(50)
                    .HasColumnName("city");

                entity.Property(e => e.CityB)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("cityB");

                entity.Property(e => e.Company)
                    .HasMaxLength(50)
                    .HasColumnName("company");

                entity.Property(e => e.CompanyB)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("companyB");

                entity.Property(e => e.Contact)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("contact")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CorpNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("corp_number");

                //entity.Property(e => e.CorpRep)
                //    .HasMaxLength(512)
                //    .HasColumnName("corp_rep");

                //entity.Property(e => e.CorpRepMobile)
                //    .HasMaxLength(50)
                //    .HasColumnName("corp_rep_mobile");

                entity.Property(e => e.Country)
                    .HasMaxLength(50)
                    .HasColumnName("country")
                    .HasDefaultValueSql("('New Zealand')");

                entity.Property(e => e.CountryB)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("countryB")
                    .HasDefaultValueSql("('New Zealand')");

                entity.Property(e => e.CreditLimit)
                    .HasColumnType("money")
                    .HasColumnName("credit_limit");

                entity.Property(e => e.CreditTerm)
                    .HasColumnName("credit_term")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CurrencyForPurchase)
                    .HasColumnName("currency_for_purchase")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CustomerAccessLevel)
                    .HasColumnName("customer_access_level")
                    .HasDefaultValueSql("((1))");

                //entity.Property(e => e.CustomerNumber)
                //    .HasMaxLength(50)
                //    .HasColumnName("customer_number");

                //entity.Property(e => e.DdAccountNumber)
                //    .HasMaxLength(255)
                //    .HasColumnName("dd_account_number");

                entity.Property(e => e.DealerLevel)
                    .HasColumnName("dealer_level")
                    .HasDefaultValueSql("((1))");

                //entity.Property(e => e.DefaultLanguage)
                //    .HasMaxLength(255)
                //    .HasColumnName("default_language");

                entity.Property(e => e.Directory)
                    .HasColumnName("directory")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Discount).HasColumnName("discount");

                //entity.Property(e => e.Dob)
                //    .HasColumnType("datetime")
                //    .HasColumnName("dob");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.Fax)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fax");

                //entity.Property(e => e.Gm)
                //    .HasMaxLength(255)
                //    .HasColumnName("gm");

                //entity.Property(e => e.GmMobile)
                //    .HasMaxLength(50)
                //    .HasColumnName("gm_mobile");

                //entity.Property(e => e.GstNumber)
                //    .HasMaxLength(50)
                //    .IsUnicode(false)
                //    .HasColumnName("gst_number");

                entity.Property(e => e.GstRate)
                    .HasColumnName("gst_rate")
                    .HasDefaultValueSql("((0.15))");

                //entity.Property(e => e.IdentityId)
                //    .HasMaxLength(50)
                //    .HasColumnName("identity_id");

                //entity.Property(e => e.InitialTerm)
                //    .HasMaxLength(50)
                //    .HasColumnName("initial_term");

                entity.Property(e => e.IsBranch).HasColumnName("is_branch");

                entity.Property(e => e.LastBranchId).HasColumnName("last_branch_id");

                entity.Property(e => e.LastPostTime)
                    .HasColumnType("datetime")
                    .HasColumnName("last_post_time")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.M1)
                    .HasColumnType("money")
                    .HasColumnName("m1");

                entity.Property(e => e.M10)
                    .HasColumnType("money")
                    .HasColumnName("m10");

                entity.Property(e => e.M11)
                    .HasColumnType("money")
                    .HasColumnName("m11");

                entity.Property(e => e.M12)
                    .HasColumnType("money")
                    .HasColumnName("m12");

                entity.Property(e => e.M2)
                    .HasColumnType("money")
                    .HasColumnName("m2");

                entity.Property(e => e.M3)
                    .HasColumnType("money")
                    .HasColumnName("m3");

                entity.Property(e => e.M4)
                    .HasColumnType("money")
                    .HasColumnName("m4");

                entity.Property(e => e.M5)
                    .HasColumnType("money")
                    .HasColumnName("m5");

                entity.Property(e => e.M6)
                    .HasColumnType("money")
                    .HasColumnName("m6");

                entity.Property(e => e.M7)
                    .HasColumnType("money")
                    .HasColumnName("m7");

                entity.Property(e => e.M8)
                    .HasColumnType("money")
                    .HasColumnName("m8");

                entity.Property(e => e.M9)
                    .HasColumnType("money")
                    .HasColumnName("m9");

                entity.Property(e => e.MainCardId).HasColumnName("main_card_id");

                //entity.Property(e => e.Midname)
                //    .HasMaxLength(128)
                //    .HasColumnName("midname");

                entity.Property(e => e.Mobile)
                    .HasMaxLength(50)
                    .HasColumnName("mobile");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("name");

                entity.Property(e => e.NameB)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("nameB");

                entity.Property(e => e.NoSysQuote).HasColumnName("no_sys_quote");

                entity.Property(e => e.Note)
                    .HasMaxLength(2500)
                    .HasColumnName("note");

                //entity.Property(e => e.OurBranch)
                //    .HasColumnName("our_branch")
                //    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.PersonalId).HasColumnName("personal_id");

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("phone");

                entity.Property(e => e.PmDdi)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("pm_ddi");

                entity.Property(e => e.PmEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("pm_email");

                entity.Property(e => e.PmMobile)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("pm_mobile");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.Postal1)
                    .HasMaxLength(50)
                    .HasColumnName("postal1");

                entity.Property(e => e.Postal2)
                    .HasMaxLength(50)
                    .HasColumnName("postal2");

                entity.Property(e => e.Postal3)
                    .HasMaxLength(50)
                    .HasColumnName("postal3");

                entity.Property(e => e.PurchaseAverage)
                    .HasColumnType("money")
                    .HasColumnName("purchase_average");

                entity.Property(e => e.PurchaseNza)
                    .HasColumnType("money")
                    .HasColumnName("purchase_nza");

                //entity.Property(e => e.Qq)
                //    .HasMaxLength(50)
                //    .HasColumnName("qq");

                //entity.Property(e => e.RegCode)
                //    .HasMaxLength(255)
                //    .HasColumnName("reg_code");

                entity.Property(e => e.RegisterDate)
                    .HasColumnType("datetime")
                    .HasColumnName("register_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Registered)
                    .IsRequired()
                    .HasColumnName("registered")
                    .HasDefaultValueSql("((1))");

                //entity.Property(e => e.ResetPwdCc)
                //    .HasMaxLength(50)
                //    .HasColumnName("reset_pwd_cc");

                //entity.Property(e => e.RootPath)
                //    .HasMaxLength(250)
                //    .HasColumnName("root_path");

                entity.Property(e => e.Sales).HasColumnName("sales");

                entity.Property(e => e.ShippingFee)
                    .HasColumnType("money")
                    .HasColumnName("shipping_fee")
                    .HasDefaultValueSql("((10))");

                entity.Property(e => e.ShortName)
                    .HasMaxLength(50)
                    .HasColumnName("short_name");

                //entity.Property(e => e.SitePass)
                //    .HasMaxLength(250)
                //    .HasColumnName("site_pass");

                entity.Property(e => e.SmDdi)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("sm_ddi");

                entity.Property(e => e.SmEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("sm_email");

                entity.Property(e => e.SmMobile)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("sm_mobile");

                entity.Property(e => e.SmName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("sm_name");

                //entity.Property(e => e.State)
                //    .HasMaxLength(255)
                //    .HasColumnName("state");

                entity.Property(e => e.StopOrder).HasColumnName("stop_order");

                entity.Property(e => e.StopOrderReason)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("stop_order_reason");

                //entity.Property(e => e.Support).HasColumnName("support");

                //entity.Property(e => e.SupportLevel).HasColumnName("support_level");

                //entity.Property(e => e.Surname)
                //    .HasMaxLength(128)
                //    .HasColumnName("surname");

                //entity.Property(e => e.TargetPoint).HasColumnName("target_point");

                //entity.Property(e => e.TargetRental).HasColumnName("target_rental");

                //entity.Property(e => e.TargetSales).HasColumnName("target_sales");

                //entity.Property(e => e.TaxNumber)
                //    .HasMaxLength(255)
                //    .HasColumnName("tax_number");

                entity.Property(e => e.TechEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("tech_email");

                //entity.Property(e => e.Tills)
                //    .HasMaxLength(50)
                //    .IsUnicode(false)
                //    .HasColumnName("tills")
                //    .HasDefaultValueSql("((1))");

                entity.Property(e => e.TotalOnlineOrderPoint)
                    .HasColumnName("total_online_order_point")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.TotalPosts).HasColumnName("total_posts");

                entity.Property(e => e.TradingName)
                    .HasMaxLength(50)
                    .HasColumnName("trading_name");

                entity.Property(e => e.TransTotal)
                    .HasColumnType("money")
                    .HasColumnName("trans_total");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("((1))");

                //entity.Property(e => e.Url)
                //    .HasMaxLength(250)
                //    .HasColumnName("url");

                entity.Property(e => e.Web)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("web");

                entity.Property(e => e.WorkingOn)
                    .HasColumnName("working_on")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Zip)
                    .HasColumnName("zip");
//              entity.Property(e => e.Updated).HasColumnName("updated").HasColumnType("bit");

            });
            modelBuilder.Entity<CodeBranch>(entity =>
            {
                entity.ToTable("code_branch");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BranchId)
                    .HasColumnName("branch_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.BranchLowStock)
                    .HasColumnName("branch_low_stock")
                    .HasDefaultValueSql("((-1))");

                entity.Property(e => e.BranchLowStockAdv)
                    .HasColumnName("branch_low_stock_adv")
                    .HasDefaultValueSql("((-1))");

                entity.Property(e => e.Code).HasColumnName("code");

                entity.Property(e => e.Inactive).HasColumnName("inactive");

                entity.Property(e => e.LsaEndDate)
                    .HasColumnName("lsa_end_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.LsaStartDate)
                    .HasColumnName("lsa_start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Price1)
                    .HasColumnName("price1")
                    .HasColumnType("money");

                entity.Property(e => e.Price2)
                    .HasColumnName("price2")
                    .HasColumnType("money");

                entity.Property(e => e.QposQtyBreak).HasColumnName("qpos_qty_break");

                entity.Property(e => e.ShelfQty)
                    .HasColumnName("shelf_qty")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.ShelfQtyAdv).HasColumnName("shelf_qty_adv");

                entity.Property(e => e.Special).HasColumnName("special");

                entity.Property(e => e.SpecialPrice)
                    .HasColumnName("special_price")
                    .HasColumnType("money");

                entity.Property(e => e.SpecialPriceEndDate)
                    .HasColumnName("special_price_end_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.SpecialPriceStartDate)
                    .HasColumnName("special_price_start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.SqaEndDate)
                    .HasColumnName("sqa_end_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.SqaStartDate)
                    .HasColumnName("sqa_start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.StockLocation)
                    .HasColumnName("stock_location")
                    .HasMaxLength(150);
            });


            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(x => new { x.Code, x.Name, x.NameCn });
                entity.ToTable("product");
                entity.Property(e => e.Code).HasColumnName("code");
                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255);
                entity.Property(e => e.NameCn)
                    .HasColumnName("name_cn")
                    .HasMaxLength(255);
                entity.Property(e => e.Brand)
                    .HasColumnName("brand")
                    .HasMaxLength(50);

                entity.Property(e => e.Cat)
                    .HasColumnName("cat")
                    .HasMaxLength(50);
                entity.Property(e => e.SCat)
                    .HasColumnName("s_cat")
                    .HasMaxLength(50);
                entity.Property(e => e.SSCat)
                    .HasColumnName("ss_cat")
                    .HasMaxLength(50);
                entity.Property(e => e.Hot)
                    .HasColumnName("hot")
                    .HasDefaultValueSql("((0))");
                entity.Property(e => e.Price)
                    .HasColumnName("price");

                entity.Property(e => e.Supplier)
                    .HasColumnName("supplier")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SupplierCode)
                    .HasColumnName("supplier_code")
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.SupplierPrice).HasColumnName("supplier_price").HasDefaultValueSql("(0)");
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

                entity.Property(e => e.Clearance).HasColumnName("clearance");

                entity.Property(e => e.Code).HasColumnName("code");

                entity.Property(e => e.ComingSoon)
                    .HasColumnName("coming_soon")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.CommissionRate).HasColumnName("commission_rate");

  //            entity.Property(e => e.CoreRange).HasColumnName("core_range");

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

                entity.Property(e => e.DateRange).HasColumnName("date_range");

                entity.Property(e => e.Disappeared).HasColumnName("disappeared");

                entity.Property(e => e.DoNotRounddown).HasColumnName("do_not_rounddown");

                entity.Property(e => e.ExchangeRate).HasColumnName("exchange_rate");

                entity.Property(e => e.ExpireDate)
                    .HasColumnName("expire_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.ForeignSupplierPrice)
                    .HasColumnName("foreign_supplier_price")
                    .HasColumnType("money");

                entity.Property(e => e.HasScale).HasColumnName("has_scale");

                entity.Property(e => e.Hidden).HasColumnName("hidden");

                entity.Property(e => e.Hot)
                    .HasColumnName("hot")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Inactive)
                    .HasColumnName("inactive")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.IncomeAccount).HasColumnName("income_account");

                entity.Property(e => e.InnerPack).HasColumnName("inner_pack");

                entity.Property(e => e.InventoryAccount).HasColumnName("inventory_account");

                entity.Property(e => e.IsBarcodeprice).HasColumnName("is_barcodeprice");

                entity.Property(e => e.IsIdCheck).HasColumnName("is_id_check");

                entity.Property(e => e.IsMemberOnly).HasColumnName("is_member_only");

                entity.Property(e => e.IsService).HasColumnName("is_service");

                entity.Property(e => e.IsSpecial).HasColumnName("is_special");

                entity.Property(e => e.IsVoidDiscount).HasColumnName("is_void_discount");

                entity.Property(e => e.IsWebsiteItem).HasColumnName("is_website_item");


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
                    .HasMaxLength(350);

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

                entity.Property(e => e.SpecialCost)
                    .HasColumnName("special_cost")
                    .HasColumnType("money");

                entity.Property(e => e.SpecialCostEndDate)
                    .HasColumnName("special_cost_end_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.SpecialCostStartDate)
                    .HasColumnName("special_cost_start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.SpecialPrice)
                    .HasColumnName("special_price")
                    .HasColumnType("money");

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
                    .HasDefaultValueSql("((0))");
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
                    .HasColumnName("item_name")
                    .HasMaxLength(500);

                entity.Property(e => e.ItemNameCn)
                    .HasColumnName("item_name_cn")
                    .HasMaxLength(150);

                entity.Property(e => e.Kit).HasColumnName("kit");

                entity.Property(e => e.Krid).HasColumnName("krid");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(255)
                    .IsUnicode(true);

                entity.Property(e => e.OrderTotal)
                    .HasColumnName("order_total")
                    .HasColumnType("money");

                entity.Property(e => e.Pack)
                    .HasColumnName("pack")
                    .HasMaxLength(100);

                entity.Property(e => e.Part)
                    .HasColumnName("part")
                    .HasDefaultValueSql("((-1))");

                entity.Property(e => e.PromoId).HasColumnName("promo_id");

                entity.Property(e => e.PromoName)
                    .HasColumnName("promo_name")
                    .HasMaxLength(128);

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

                entity.Property(e => e.TaxCode)
                    .HasColumnName("tax_code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxRate)
                    .HasColumnName("tax_rate")
                    .HasDefaultValueSql("((0))");
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

                entity.Property(e => e.WebOrderStatus).HasColumnName("web_order_status");

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
                    .IsUnicode(true);

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
            modelBuilder.Entity<ProductDetails>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("product_details");

                entity.HasIndex(e => e.Code)
                    .HasName("IDX_product_details_code")
                    .IsUnique();

                entity.Property(e => e.Code)
                    .HasColumnName("code")
                    .ValueGeneratedNever();

                entity.Property(e => e.Advice)
                    .HasColumnName("advice")
                    .HasMaxLength(550);

                entity.Property(e => e.Details)
                    .HasColumnName("details")
                    .HasMaxLength(2550);

                entity.Property(e => e.Directions)
                    .HasColumnName("directions")
                    .HasMaxLength(550);

                entity.Property(e => e.Highlight)
                    .HasColumnName("highlight")
                    .HasColumnType("ntext");

                entity.Property(e => e.Ingredients)
                    .HasColumnName("ingredients")
                    .HasMaxLength(550);

                entity.Property(e => e.Manufacture)
                    .HasColumnName("manufacture")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Pic)
                    .HasColumnName("pic")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Rev)
                    .HasColumnName("rev")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Shipping)
                    .HasColumnName("shipping")
                    .HasMaxLength(550);

                entity.Property(e => e.Spec)
                    .HasColumnName("spec")
                    .HasColumnType("ntext");

                entity.Property(e => e.Warranty)
                    .HasColumnName("warranty")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });
            modelBuilder.Entity<PromotionList>(entity =>
            {
                entity.HasKey(e => e.PromoId);
                entity.ToTable("promotion_list");
                entity.Property(e => e.PromoId).HasColumnName("promo_id");
                entity.Property(e => e.PromoDesc).HasColumnName("promo_desc");
                entity.Property(e => e.PromoType).HasColumnName("promo_type");
                entity.Property(e => e.PromoStartDate).HasColumnName("promo_start_date");
                entity.Property(e => e.PromoEndDate).HasColumnName("promo_end_date");
                entity.Property(e => e.PromoActive).HasColumnName("promo_active");
                entity.Property(e => e.PromoMemberOnly).HasColumnName("promo_member_only");
                entity.Property(e => e.PromoDay1).HasColumnName("promo_day1");
                entity.Property(e => e.PromoDay2).HasColumnName("promo_day2");
                entity.Property(e => e.PromoDay3).HasColumnName("promo_day3");
                entity.Property(e => e.PromoDay4).HasColumnName("promo_day4");
                entity.Property(e => e.PromoDay5).HasColumnName("promo_day5");
                entity.Property(e => e.PromoDay6).HasColumnName("promo_day6");
                entity.Property(e => e.PromoDay7).HasColumnName("promo_day7");
                entity.Property(e => e.SpecialPrice).HasColumnName("special_price");
                entity.Property(e => e.DiscountPercentage).HasColumnName("discount_percentage");
                entity.Property(e => e.FreeQtyRequiredQty).HasColumnName("free_qty_required_qty");
                entity.Property(e => e.FreeQtyRewardQty).HasColumnName("free_qty_reward_qty");
                entity.Property(e => e.VolumnDiscountQty).HasColumnName("volumn_discount_qty");
                entity.Property(e => e.VolumnDiscountPriceTotal).HasColumnName("volumn_discount_price_total");
                entity.Property(e => e.FreeItemRequiredQty).HasColumnName("free_item_required_qty");
                entity.Property(e => e.FreeItemRequiredItemCode).HasColumnName("free_item_required_item_code");
                entity.Property(e => e.FreeItemRewardQty).HasColumnName("free_item_reward_qty");
                entity.Property(e => e.PromoCreateDate).HasColumnName("promo_create_date");
                entity.Property(e => e.PromoCreateBy).HasColumnName("promo_create_by");
                entity.Property(e => e.PromoBranchId).HasColumnName("promo_branch_id");
                entity.Property(e => e.Limit).HasColumnName("limit");
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
                    .HasMaxLength(500);

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

                entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");

                entity.Property(e => e.PromoId).HasColumnName("promo_id");

                entity.Property(e => e.PromoName)
                    .HasColumnName("promo_name")
                    .HasMaxLength(128);

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

                entity.Property(e => e.TaxCode)
                    .HasColumnName("tax_code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxRate)
                    .HasColumnName("tax_rate")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Ticket)
                    .HasColumnName("ticket")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Used).HasColumnName("used");
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
                entity.Property(e => e.Points).HasColumnName("points").HasColumnType("int").HasDefaultValueSql("(0)");

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

                entity.Property(e => e.UploadedActivata)
                    .IsRequired()
                    .HasColumnName("uploaded_activata")
                    .HasDefaultValueSql("((1))");
            });
            modelBuilder.Entity<InvoiceFreight>(entity =>
            {
                entity.ToTable("invoice_freight");

                entity.HasIndex(e => e.InvoiceNumber)
                    .HasName("IDX_invoice_freight_invoice_number");

                entity.HasIndex(e => e.Ticket)
                    .HasName("IDX_invoice_freight_ticket");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.InvoiceNumber).HasColumnName("invoice_number");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("money");

                entity.Property(e => e.ShipDesc)
                    .IsRequired()
                    .HasColumnName("ship_desc")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShipId).HasColumnName("ship_id");

                entity.Property(e => e.ShipName)
                    .IsRequired()
                    .HasColumnName("ship_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Ticket)
                    .IsRequired()
                    .HasColumnName("ticket")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });
            modelBuilder.Entity<StockQty>(entity =>
            {
                entity.ToTable("stock_qty");

                entity.HasIndex(e => e.BranchId)
                    .HasName("IDX_stock_qty_branch_id");

                entity.HasIndex(e => e.Code)
                    .HasName("IDX_stock_qty_code");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AllocatedStock)
                    .HasColumnName("allocated_stock")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.AverageCost)
                    .HasColumnName("average_cost")
                    .HasColumnType("money")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.BranchId)
                    .HasColumnName("branch_id")
                    .HasDefaultValueSql("(1)");

                entity.Property(e => e.Code).HasColumnName("code");

                entity.Property(e => e.LastStock).HasColumnName("last_stock");

                entity.Property(e => e.QposPrice)
                    .HasColumnName("qpos_price")
                    .HasColumnType("money")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.Qty).HasColumnName("qty");

                entity.Property(e => e.SpEndDate)
                    .HasColumnName("sp_end_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SpStartDate)
                    .HasColumnName("sp_start_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SpecialPrice)
                    .HasColumnName("special_price")
                    .HasColumnType("money");

                entity.Property(e => e.SupplierPrice)
                    .HasColumnName("supplier_price")
                    .HasColumnType("money")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.WarningStock).HasColumnName("warning_stock");
            });
            modelBuilder.Entity<StoreSpecial>(entity =>
            {
                entity.ToTable("store_special");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BranchId).HasColumnName("branch_id");

                entity.Property(e => e.Code).HasColumnName("code");

                entity.Property(e => e.Cost)
                    .HasColumnName("cost")
                    .HasColumnType("money");

                entity.Property(e => e.CostEndDate)
                    .HasColumnName("cost_end_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CostStartDate)
                    .HasColumnName("cost_start_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Enabled).HasColumnName("enabled");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("money");

                entity.Property(e => e.PriceEndDate)
                    .HasColumnName("price_end_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PriceStartDate)
                    .HasColumnName("price_start_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });
            modelBuilder.Entity<UpdatedBranch>(entity =>
            {
                entity.ToTable("updated_branch");

                entity.HasIndex(e => e.Id)
                    .HasName("IDX_updated_branch_id");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.BranchId).HasColumnName("branch_id")
                        .IsRequired()
                        .HasDefaultValue("1");
                entity.Property(e => e.hasUpdated).HasColumnName("has_updated").IsRequired().HasDefaultValue(false);
                entity.Property(e => e.HasCreated).HasColumnName("has_created").IsRequired().HasDefaultValue(false);
                entity.Property(e => e.HasProcessed).HasColumnName("has_processed").IsRequired().HasDefaultValue(false);

            });
            modelBuilder.Entity<UpdatedItem>(entity =>
            {
                entity.ToTable("updated_item");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ItemCode).HasColumnName("item_code");
                entity.Property(e => e.BranchId).HasColumnName("branch_id");
                entity.Property(e => e.Delete).HasColumnName("del").HasDefaultValueSql("(0)"); ;
                entity.Property(e => e.DateUpdated).HasColumnName("date_updated").HasDefaultValueSql("getdate()");
                entity.Property(e => e.TimeStamp).HasColumnName("time_stamp").HasColumnType("timestamp");
            });
            modelBuilder.Entity<UpdatedCard>(entity =>
            {
                entity.ToTable("updated_card");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CardId).HasColumnName("card_id");
                entity.Property(e => e.Barcode).HasColumnName("barcode");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.BranchId).HasColumnName("branch_id");
                entity.Property(e => e.Delete).HasColumnName("del").HasDefaultValueSql("(0)");
                entity.Property(e => e.DateUpdated).HasColumnName("date_updated").HasDefaultValueSql("getdate()");
                entity.Property(e => e.TimeStamp).HasColumnName("time_stamp").HasColumnType("timestamp");
            });

            modelBuilder.Entity<UpdatedCategory>(entity =>
            {
                entity.ToTable("updated_category");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.BranchId).HasColumnName("branch_id");
                entity.Property(e => e.DateUpdated).HasColumnName("date_updated").HasDefaultValueSql("getdate()");
                entity.Property(e => e.TimeStamp).HasColumnName("time_stamp").HasColumnType("timestamp");
            });

			modelBuilder.Entity<Catalog>(entity =>
			{
				entity.ToTable("catalog");
                entity.Property(e => e.Seq).HasColumnName("seq").IsRequired(); ;
				entity.Property(e => e.Cat).HasColumnName("cat");
				entity.Property(e => e.SCat).HasColumnName("s_cat");
				entity.Property(e => e.SSCat).HasColumnName("ss_cat");
			});

			modelBuilder.Entity<UpdatedPromotion>(entity =>
            {
                entity.ToTable("updated_promotion");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.BranchId).HasColumnName("branch_id");
                entity.Property(e => e.DateUpdated).HasColumnName("date_updated").HasDefaultValueSql("getdate()");
                entity.Property(e => e.TimeStamp).HasColumnName("time_stamp").HasColumnType("timestamp");
            });

            modelBuilder.Entity<UpdatedButton>(entity =>
            {
                entity.ToTable("updated_button");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ButtonId).HasColumnName("button_id");
                //entity.Property(e => e.IsDelete).HasColumnName("is_delete");
                entity.Property(e => e.BranchId).HasColumnName("branch_id");
                entity.Property(e => e.DateUpdated).HasColumnName("date_updated").HasDefaultValueSql("getdate()");
                entity.Property(e => e.TimeStamp).HasColumnName("time_stamp").HasColumnType("timestamp");
            });

            modelBuilder.Entity<Models.Enum>(entity => {
                entity.ToTable("enum");
                entity.Property(e => e.Class).HasColumnName("class");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<Models.PromotionGroup>(entity => {
                entity.ToTable("promotion_group");
                entity.Property(e => e.PromoId).HasColumnName("promo_id");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Barcode).HasColumnName("barcode");
                entity.Property(e => e.PromoType).HasColumnName("promo_type");
            });
            modelBuilder.Entity<PromotionBranch>(entity => {
                entity.ToTable("promotion_branch");
            //  entity.HasKey("id");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PromoId).HasColumnName("promo_id");
                entity.Property(e => e.BranchId).HasColumnName("branch_id");            
            });
            modelBuilder.Entity<Settings>(entity => {
                entity.ToTable("settings");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Cat).HasColumnName("cat");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<WorkTime>(entity => {
                entity.ToTable("work_time");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CardId).HasColumnName("card_id");
                entity.Property(e => e.RecordTime).HasColumnName("record_time");
                entity.Property(e => e.IsCheckin).HasColumnName("is_checkin");//.HasDefaultValueSql("(1)");
                entity.Property(e => e.Hours).HasColumnName("hours");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Barcode).HasColumnName("barcode");
                entity.Property(e => e.BranchId).HasColumnName("branch_id");
            });

            modelBuilder.Entity<Image>(entity => {
                entity.ToTable("image");
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Code).HasColumnName("code");
                entity.Property(e => e.Url).HasColumnName("url");
                entity.Property(e => e.MainPic).HasColumnName("main_pic");
                entity.Property(e => e.PicToBinary).HasColumnName("pic_to_binary");
            });

            modelBuilder.Entity<TranDetail>(entity => {
                entity.ToTable("tran_detail");
                entity.HasKey(e => e.Kid);
                entity.Property(e => e.Kid).HasColumnName("Kid");
                entity.Property(e => e.PaymentRef).HasColumnName("payment_ref");
            });
        }
    }
}
