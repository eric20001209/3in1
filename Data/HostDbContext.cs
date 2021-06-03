using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST_Multi.Data
{
    public class HostDbContext : DbContext
    {
        public HostDbContext()
        {

        }
        public HostDbContext(DbContextOptions<HostDbContext> options) : base(options)
        { 
        
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
        
        }

        public DbSet<HostTenant> HostTenants { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");
            modelBuilder.Entity<HostTenant>(entity =>
            {
                entity.ToTable("db_list");
                //entity.HasKey("Id");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Connstr).HasColumnName("conn_str");
                entity.Property(e => e.Removeable).HasColumnName("removable").HasDefaultValueSql("(0)");
                entity.Property(e => e.InstallDbId).HasColumnName("install_db_id").HasDefaultValueSql("(0)");
                entity.Property(e => e.Creator).HasColumnName("creator");
                entity.Property(e => e.CreateDate).HasColumnName("create_date")
                    .HasColumnType("datetime").HasDefaultValueSql("(getdate())");
                entity.Property(e => e.UpdateDate).HasColumnName("update_date").HasColumnType("datetime");

            });
        }
    }
}
