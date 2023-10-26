using ECom_Razor.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ECom_Razor.Data
{
    public class RazorDbContext : DbContext
    {
        public RazorDbContext(DbContextOptions<RazorDbContext> option) : base(option)
        {

        }
        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Anime", DisplayOrder = 2 },
                new Category { Id = 3, Name = "SciFi", DisplayOrder = 3 },
                new Category { Id = 4, Name = "History", DisplayOrder = 4 }
                );
        }
    }
}
