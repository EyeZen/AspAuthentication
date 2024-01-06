using AspAuthentication.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspAuthentication.Data
{
    public class ApplicationDbContext : IdentityUserContext<ApplicationUser>
    {
        private readonly IConfiguration _config;

        public DbSet<Page> Pages => Set<Page>();
        //public DbSet<ApplicationUser> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration config)
            : base(options)
        {
            _config = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var hasher = new PasswordHasher<ApplicationUser>();

            var adminEmail = _config.GetSection("SiteSettings")["AdminEmail"];
            var adminPassword = _config.GetSection("SiteSettings")["AdminPassword"];

            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = "80c8b6b1-e2b6-45e8-b044-8f2178a90111",
                    UserName = "admin",
                    NormalizedUserName = adminEmail!.ToUpper(),
                    PasswordHash = hasher.HashPassword(null!, adminPassword!),
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpper(),
                    Role = Auth.Enums.Role.Admin,
                }
            );
        }
    }
}
