using Microsoft.EntityFrameworkCore;

namespace BobsBetting.DBModels 
{
    public class User
    {
        public int Id { get; set;}
        public string Username { get; set;}
        public string Email { get; set;}
        public int Chips { get; set;} = 1000;
        public string Password { get; set;}
    }

        class BBDb : DbContext
    {
        public BBDb(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enforce unique constraint for Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Enforce unique constraint for Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
