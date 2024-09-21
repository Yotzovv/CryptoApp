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
        
       
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Currency>().HasKey(x => x.Id);

            builder.Entity<Currency>().Property(x => x.CurrentPrice).HasPrecision(18, 6);
            builder.Entity<Currency>().Property(x => x.CurrentValue).HasPrecision(18, 6);
            builder.Entity<Currency>().Property(x => x.InitialBuyPrice).HasPrecision(18, 6);
            builder.Entity<Currency>().Property(x => x.InitialValue).HasPrecision(18, 6);
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
