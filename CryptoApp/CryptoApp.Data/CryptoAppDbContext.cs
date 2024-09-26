using System.Runtime.InteropServices;
using CryptoApp.Data.Models;

namespace CryptoApp.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

public class CryptoAppDbContext : IdentityDbContext<AspNetUser, IdentityRole<Guid>, Guid>
    {
        public CryptoAppDbContext() { }

        public CryptoAppDbContext(DbContextOptions<CryptoAppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        
        public virtual DbSet<Portfolio> Portfolios { get; set; }
       
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AspNetUser>(user =>
            {
                user.HasKey(x => x.Id);
                user.Property(x => x.FirstName).HasMaxLength(50);
                user.Property(x => x.LastName).HasMaxLength(50);
                user.Property(x => x.IsActive).HasDefaultValue(true);
                
                user.HasOne(p => p.Portfolio)
                    .WithOne(u => u.User)
                    .HasForeignKey<Portfolio>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);                    
            });


            builder.Entity<Portfolio>(portfolio =>
            {
                portfolio.HasKey(x => x.Id);
                
                portfolio.HasOne(x => x.User)
                    .WithOne(x => x.Portfolio)
                    .HasForeignKey<Portfolio>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                portfolio.HasMany(x => x.Currencies)
                    .WithOne(x => x.Portfolio)
                    .HasForeignKey(x => x.PortfolioId);
                
                portfolio.Property(x => x.InitialPortfolioValue).HasPrecision(18, 6);
                portfolio.Property(x => x.CurrentPortfolioValue).HasPrecision(18, 6);
                portfolio.Property(x => x.InitialPortfolioValue).HasPrecision(18, 6);
            });
            
            
            builder.Entity<Currency>(currency =>
            {
                currency.HasKey(x => x.Id);
                currency.Property(x => x.CurrentPrice).HasPrecision(18, 6);
                currency.Property(x => x.CurrentValue).HasPrecision(18, 6);
                currency.Property(x => x.InitialBuyPrice).HasPrecision(18, 6);
                currency.Property(x => x.InitialValue).HasPrecision(18, 6);

                currency.HasIndex(c => 
                        new { c.Coin, c.PortfolioId })
                    .IsUnique();
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);

            var osPath = GetOsDbPath();

            builder.UseSqlServer(osPath);
        }

        private static string GetOsDbPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Server=.\\SQLEXPRESS;Database=CryptoAppDB;Integrated Security=True;Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=True";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return @"Server=localhost,1433;Database=CryptoAppDB;User Id=sa;Password=reallyStrongPwd123;TrustServerCertificate=true";

            return "Running on other OS";
        }
}
