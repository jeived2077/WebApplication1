using Microsoft.EntityFrameworkCore;
namespace WebApplication1
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    public class Database : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;

        public Database()
        {
            Database.EnsureCreated();   // гарантируем, что БД создана
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=helloapp456.db");
        }
    }
}
