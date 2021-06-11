using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using eCommerce_API.Models;
using Microsoft.Extensions.Configuration;
using eCommerce_API_RST_Multi.Data;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Data.SqlClient;

namespace eCommerce_API.Data
{
    public partial class FreightContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly HostDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IDbConnection DbConnection { get; set; }
        private int HostId { get; set; }

        public FreightContext()
        {
        }

        public FreightContext(DbContextOptions<FreightContext> options, IConfiguration configuration, HostDbContext context,
                        IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            var hostId = _httpContextAccessor.HttpContext.GetRouteValue("hostId").ToString();
            // httpContextAccessor.HttpContext.Request.RouteValues["hostId"];    //this is for .net core 3

            this._configuration = configuration;
            HostId = int.Parse(hostId);
        }

        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<FreightSettings> FreightSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
  //        if (!optionsBuilder.IsConfigured)
            {
                //              #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                //             optionsBuilder.UseSqlServer("Server=192.168.1.248\\sql2014;Database=wanfang_cloud14;User Id=eznz;password=9seqxtf7");
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

            modelBuilder.Entity<Settings>(entity =>
            {
                entity.ToTable("settings");

                entity.HasIndex(e => e.Cat)
                    .HasName("IDX_settings_cat");

                entity.HasIndex(e => e.Hidden)
                    .HasName("IDX_settings_hidden");

                entity.HasIndex(e => e.Name)
                    .HasName("IDX_settings_name");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Access).HasColumnName("access");

                entity.Property(e => e.BoolValue)
                    .IsRequired()
                    .HasColumnName("bool_value")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.Cat)
                    .HasColumnName("cat")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Hidden)
                    .IsRequired()
                    .HasColumnName("hidden")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasMaxLength(1024)
                    .IsUnicode(false);
            });
            modelBuilder.Entity<FreightSettings>(entity =>
            {
                entity.ToTable("freight_settings");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.FreeshippingActiveAmount)
                    .HasColumnName("freeshipping_active_amount")
                    .HasColumnType("money");

                entity.Property(e => e.Freight)
                    .HasColumnName("freight")
                    .HasColumnType("money");

                entity.Property(e => e.Region)
                    .HasColumnName("region")
                    .HasMaxLength(250);
            });

        }
    }
}
