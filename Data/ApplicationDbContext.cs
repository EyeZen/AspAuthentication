using AspAuthentication.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AspAuthentication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Page> Pages => Set<Page>();
        public DbSet<ApplicationUser> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
