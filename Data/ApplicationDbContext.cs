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

        // A User table is alread created by IdentityUserContext, hence below line is not required
        //public DbSet<ApplicationUser> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration config)
            : base(options)
        {
            _config = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // How default password hasher works: https://stackoverflow.com/questions/20621950/asp-net-identitys-default-password-hasher-how-does-it-work-and-is-it-secure
            // VerifyHashedPassword: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1.verifyhashedpassword?view=aspnetcore-8.0#microsoft-aspnetcore-identity-passwordhasher-1-verifyhashedpassword(-0-system-string-system-string)
            var hasher = new PasswordHasher<ApplicationUser>();

            var adminEmail = _config.GetSection("SiteSettings")["AdminEmail"];
            var adminPassword = _config.GetSection("SiteSettings")["AdminPassword"];

            // Seed database (need to Update-Database to take effect)
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
